#pragma unmanaged
#include "orderscan.h"
#include <vector_types.h>
#include <vector>

struct cu_statistic_base
{
  float delta;
  int time;
  int count;
  __device__ cu_statistic_base& operator += (const cu_statistic_base& stat)
  {
    delta += stat.delta;
    time += stat.time;
    count += stat.count;
    return *this;
  }
};

struct cu_statistic
{
  cu_statistic_base profit;
  cu_statistic_base loss;
  cu_statistic_base timeout;
  __device__ cu_statistic& operator += (const cu_statistic& stat)
  {
    this->profit += stat.profit;
    this->loss += stat.loss;
    this->timeout += stat.timeout;
    return *this;
  }
}; 

__device__ float kernelBuy(float2 open, float2 close)
{
  return close.x - open.y;
}

__device__ float kernelSell(float2 open, float2 close)
{
  return open.x - close.y;
}

struct position
{
  float delta;
  int time;
};

enum position_type
{
    BUY, SELL
};

__device__ position kernelSinglePosition(const float2* quotes, int index, int timeout, float takeprofit, float stoploss, position_type ptype)
{
  position pos;
  for (int i = 1; i < timeout; i++)
  {
    float delta = ptype == BUY ? kernelBuy(quotes[index], quotes[index + i]) : kernelSell(quotes[index], quotes[index + i]);
    if (delta >= takeprofit || delta <= -stoploss)
    {
      pos.delta = delta;
      pos.time = i;
      return pos;
    }
  }
  pos.delta = ptype == BUY ? kernelBuy(quotes[index], quotes[index + timeout]) : kernelSell(quotes[index], quotes[index + timeout]);
  pos.time = timeout;
  return pos;
}

__device__ cu_statistic kernelSingleStat(const float2* quotes, int index, int timeout, float takeprofit, float stoploss, char* wstat, position_type ptype)
{
  cu_statistic stat = { {0,0,0}, {0,0,0}, {0,0,0} };
  position pos = kernelSinglePosition(quotes, index, timeout, takeprofit, stoploss, ptype);
  if (pos.time == timeout)
  {
    stat.timeout.delta = pos.delta;
    stat.timeout.time = pos.time;
    stat.timeout.count = 1;
    wstat[index] = 0;
  }
  else if (pos.delta >= takeprofit)
  {
    stat.profit.delta = pos.delta;
    stat.profit.time = pos.time;
    stat.profit.count = 1;
    wstat[index] = 1;
  }
  else if (pos.delta <= -stoploss)
  {
    stat.loss.delta = pos.delta;
    stat.loss.time = pos.time;
    stat.loss.count = 1;
    wstat[index] = 0;
  }
  return stat;
}

__device__ void kernelOrderScan(const float2* quotes, int count, int timeout, float takeprofit, float stoploss, cu_statistic* stat, char* wstat, position_type ptype)
{
  __shared__ cu_statistic block_cache[native::ThreadCount];
  int index = (blockIdx.y * gridDim.x + blockIdx.x) * blockDim.x * blockDim.y + threadIdx.y * blockDim.x + threadIdx.x;
  int idx_thread = threadIdx.y * blockDim.x + threadIdx.x;
  if (index < (count - timeout))
  {
    block_cache[idx_thread] = kernelSingleStat(quotes, index, timeout, takeprofit, stoploss, wstat, ptype);
  }
  else
  {
    cu_statistic empty_stat = { {0,0,0}, {0,0,0}, {0,0,0} };
    block_cache[idx_thread] = empty_stat;
  }

  __syncthreads();

  int thread_count = blockDim.x * blockDim.y;
  if (thread_count < native::ThreadCount)
  {
    if (idx_thread == 0)
    {
      for (int i = 1; i < thread_count; i++)
      {
        block_cache[0] += block_cache[i];
      }
    }
    __syncthreads();
  }
  else
  {
    for (int t = native::ThreadCount / 2; t > 0; t /= 2)
    {
      if (idx_thread < t)
      {
        block_cache[idx_thread] += block_cache[idx_thread + t];
      }
      __syncthreads();
    }
  }

  if (idx_thread == 0)
  {
    int idx_block = blockIdx.y * gridDim.x + blockIdx.x;
    stat[idx_block] = block_cache[0];
  }
}

__global__ void kernelOrderBuyScan(const float2* quotes, int count, int timeout, float takeprofit, float stoploss, cu_statistic* stat, char* wstat)
{
  kernelOrderScan(quotes, count, timeout, takeprofit, stoploss, stat, wstat, BUY);
}

__global__ void kernelOrderSellScan(const float2* quotes, int count, int timeout, float takeprofit, float stoploss, cu_statistic* stat, char* wstat)
{
  kernelOrderScan(quotes, count, timeout, takeprofit, stoploss, stat, wstat, SELL);
}

namespace native
{
  CudaOrderScan::CudaOrderScan( quote* quotes, int count, int timeout )
    : Count(count)
    , Timeout(timeout)
    , cuda_dim(count - timeout)
  {
    std::vector<float2> prepared_quotes(count);
    for (int i = 0; i < count; i++)
    {
      prepared_quotes[i].x = quotes[i].low; // bid - цена по которой мы продадим
      prepared_quotes[i].y = quotes[i].high; // ask - цена по которой мы купим
      // TODO: Ќеплохо бы еще учесть spread.
    }
    dev_quotes.reset(CudaCreater<float2>(count), CudaDeleter<float2>);
    cudaMemcpy(dev_quotes.get(), prepared_quotes.data(), sizeof(float2) * count, cudaMemcpyHostToDevice);
    dev_stat.reset(CudaCreater<cu_statistic>(cuda_dim.Nblocks()), CudaDeleter<cu_statistic>);
    dev_wstat.reset(CudaCreater<char>(count - timeout), CudaDeleter<char>);
  }

  void CudaOrderScan::ScanBuy( float takeprofit, float stoploss )
  {
    kernelOrderBuyScan<<<*cuda_dim.gridSize, *cuda_dim.blockSize>>>(dev_quotes.get(), Count, Timeout, takeprofit, stoploss, dev_stat.get(), dev_wstat.get());
  }

  void CudaOrderScan::ScanSell( float takeprofit, float stoploss )
  {
    kernelOrderSellScan<<<*cuda_dim.gridSize, *cuda_dim.blockSize>>>(dev_quotes.get(), Count, Timeout, takeprofit, stoploss, dev_stat.get(), dev_wstat.get());
  }

  statistic CudaOrderScan::Scan( float takeprofit, float stoploss, ScanFunc func )
  {
    (this->*func)(takeprofit, stoploss);
    std::vector<cu_statistic> partial_stat(cuda_dim.Nblocks());
    cudaMemcpy(partial_stat.data(), dev_stat.get(), sizeof(cu_statistic) * cuda_dim.Nblocks(), cudaMemcpyDeviceToHost);
    statistic stat;
    for (int i = 0; i < partial_stat.size(); i++)
    {
      stat.Add(partial_stat[i]);
    }
    // ¬ычисл€ем число окон.
    std::vector<char> wstat(Count - Timeout);
    cudaMemcpy(wstat.data(), dev_wstat.get(), Count - Timeout, cudaMemcpyDeviceToHost);
    bool inwindow = false;
    for (int i = 0; i < (Count - Timeout); i++)
    {
      if (wstat[i])
      {
        inwindow = true;
      }
      else
      {
        if (inwindow)
        {
          stat.wcount++;
          inwindow = false;
        }
      }
    }
    if (inwindow)
    {
      stat.wcount++;
      inwindow = false;
    }
    return stat;
  }

  void statistic::Add( const cu_statistic& stat )
  {
    this->profit.Add(stat.profit);
    this->loss.Add(stat.loss);
    this->timeout.Add(stat.timeout);
  }

  void statistic::statistic_base::Add( const cu_statistic_base& stat )
  {
    avg_delta += stat.delta;
    avg_wait += stat.time;
    count += stat.count;
  }

}