#pragma unmanaged
#include "dft.h"

__global__ void kernelCreateTurner(float* turners, int n)
{
  int idx = (blockIdx.y * gridDim.x + blockIdx.x) * blockDim.x * blockDim.y + threadIdx.y * blockDim.x + threadIdx.x;
  if (idx < n)
  {
    float a = 6.283185307179586476925286766559f * __int2float_rn(idx) / __int2float_rn(n);
    float* cs = turners + 2 * idx;
    sincosf(a, cs + 1, cs);
  }
}

__global__ void kernelHarmonic(float* x, float* coef, int k, int n, float* turners)
{
  __shared__ float2 block_cache[native::ThreadCount];
  int idx = (blockIdx.y * gridDim.x + blockIdx.x) * blockDim.x * blockDim.y + threadIdx.y * blockDim.x + threadIdx.x;
  int idx_thread = threadIdx.y * blockDim.x + threadIdx.x;
  if (idx < n)
  {
    int idx_turner = 2 * ( ( (long long)(idx) * (long long)(k) ) % (long long)(n) );
    block_cache[idx_thread] = make_float2(x[idx] * turners[idx_turner], - x[idx] * turners[idx_turner + 1]);
  }
  else
  {
    block_cache[idx_thread] = make_float2(0, 0);
  }

  __syncthreads();

  int thread_count = blockDim.x * blockDim.y;
  if (thread_count < native::ThreadCount)
  {
    if (idx_thread == 0)
    {
      for (int i = 1; i < thread_count; i++)
      {
        block_cache[0].x += block_cache[i].x;
        block_cache[0].y += block_cache[i].y;
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
        block_cache[idx_thread].x += block_cache[idx_thread + t].x;
        block_cache[idx_thread].y += block_cache[idx_thread + t].y;
      }
      __syncthreads();
    }
  }

  if (idx_thread == 0)
  {
    int idx_block = blockIdx.y * gridDim.x + blockIdx.x;
    coef[2 * idx_block + 0] = block_cache[0].x;
    coef[2 * idx_block + 1] = block_cache[0].y;
  }
}

namespace native
{
  CudaDFourier::CudaDFourier( float* x, int n ) : N(n), cuda_dim(n)
  {
    devX.reset(CudaCreater<float>(n), CudaDeleter<float>);
    cudaMemcpy(devX.get(), x, sizeof(float) * n, cudaMemcpyHostToDevice);
    devTurners.reset(CudaCreater<float>(2*n), CudaDeleter<float>);
    kernelCreateTurner<<<*cuda_dim.gridSize, *cuda_dim.blockSize>>>(devTurners.get(), N);
  }

  Complex CudaDFourier::Harmonic( int k )
  {
    Complex coef(0, 0);
    const int Nblocks = cuda_dim.gridSize->x * cuda_dim.gridSize->y;
    std::tr1::shared_ptr<float> dev_coefs(CudaCreater<float>(2 * Nblocks), CudaDeleter<float>);
    kernelHarmonic<<<*cuda_dim.gridSize, *cuda_dim.blockSize>>>(devX.get(), dev_coefs.get(), k, N, devTurners.get());
    
    std::vector<float> coefs(2 * Nblocks);
    cudaMemcpy(&coefs[0], dev_coefs.get(), sizeof(float) * (2 * Nblocks), cudaMemcpyDeviceToHost);
    for (int i = 0; i < Nblocks; i++)
    {
      coef += Complex(coefs[2 * i], coefs[2 * i + 1]);
    }
    return coef / (float)N;
  }

}