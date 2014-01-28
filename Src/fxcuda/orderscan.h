#pragma once
#include <memory>
#include "cubase.h"

struct float2; // x - bid, y - ask (ask >= bid)
struct int2; // x - delta, y - time
struct cu_statistic_base;
struct cu_statistic;

namespace native
{
  struct quote
  {
    float open, high, low, close; // цены, переведенные уже пункты!
  };
  struct statistic
  {
    struct statistic_base
    {
      statistic_base() : avg_delta(0), avg_wait(0), count(0) {}
      double avg_delta;
      double avg_wait;
      int count;
      void Add(const cu_statistic_base& stat);
    };
    
    statistic() : wcount(0) {}
    statistic_base profit;
    statistic_base loss;
    statistic_base timeout;
    int wcount;
    void Add(const cu_statistic& stat);
  };
 
  class CudaOrderScan
  {
  public:
    CudaOrderScan(quote* quotes, int count, int timeout);
    
    typedef void (CudaOrderScan::*ScanFunc)(float takeprofit, float stoploss);
    statistic Scan(float takeprofit, float stoploss, ScanFunc func);
    void ScanBuy(float takeprofit, float stoploss);
    void ScanSell(float takeprofit, float stoploss);
  private:
    const int Count, Timeout;
    std::tr1::shared_ptr<float2> dev_quotes;
    std::tr1::shared_ptr<cu_statistic> dev_stat;
    std::tr1::shared_ptr<char> dev_wstat;
    CudaDim cuda_dim;
  };
}
