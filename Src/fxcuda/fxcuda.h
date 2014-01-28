// fxcuda.h

#pragma once

#pragma unmanaged
#include "devprop.h"
#include "dft.h"
#include "orderscan.h"

#pragma managed
using namespace System;
using namespace System::Runtime::InteropServices;
using namespace FxMath;
using namespace FxMath::Orders;
using namespace System::Threading;

namespace FxCuda
{
  public ref struct DeviceProp
  {
    String^ Name;
    long long GlobalMem;
    long long SharedMem;
    long long ConstMem;
    int ThreadsPerBlock;
    array<int>^ ThreadsDim;
    array<int>^ GridSize;
    int ClockRate;
    int Major;
    int Minor;
    int ProcessorCount;
    int CurrentKernels;
    int MemoryClockRate;
    int ThreadsPerProcessor;
  };

  public ref struct Cuda sealed abstract
  {
    static property int DeviceCount
    {
      int get() { return native::GetDeviceCount(); }
    }

    static property int CurrentDevice
    {
      int get() { return native::CurrentDevice(); }
    }

    static DeviceProp^ GetDeviceProp(int dev)
    {
      DeviceProp^ dev_prop = nullptr;
      std::tr1::shared_ptr<native::DeviceProp> prop = native::GetDeviceProp(dev);
      if (prop)
      {
        dev_prop = gcnew DeviceProp();
        dev_prop->Name = Marshal::PtrToStringAnsi(static_cast<IntPtr>(const_cast<char*>(prop->Name.c_str())));
        dev_prop->GlobalMem = prop->GlobalMem;
        dev_prop->SharedMem = prop->SharedMem;
        dev_prop->ConstMem = prop->ConstMem;
        dev_prop->ThreadsPerBlock = prop->ThreadsPerBlock;
        dev_prop->ThreadsDim = gcnew array<int>(3);
        pin_ptr<int> ptr_tdim = &(dev_prop->ThreadsDim[0]);
        memcpy(ptr_tdim, prop->ThreadsDim, sizeof(prop->ThreadsDim));
        dev_prop->GridSize = gcnew array<int>(3);
        pin_ptr<int> ptr_grid = &(dev_prop->GridSize[0]);
        memcpy(ptr_grid, prop->GridSize, sizeof(prop->GridSize));
        dev_prop->ClockRate = prop->ClockRate;
        dev_prop->Major = prop->Major;
        dev_prop->Minor = prop->Minor;
        dev_prop->ProcessorCount = prop->ProcessorCount;
        dev_prop->CurrentKernels = prop->CurrentKernels;
        dev_prop->MemoryClockRate = prop->MemoryClockRate;
        dev_prop->ThreadsPerProcessor = prop->ThreadsPerProcessor;
      }
      return dev_prop;
    }
  };


	public ref class CudaDFourier : IFourierTransform
	{
  public:
    CudaDFourier(IWindow^ window) : Window(window) { }

    virtual array<Complex>^ Transform(array<float>^ y)
    {
      array<Complex>^ coefs = gcnew array<Complex>(y->Length);
      float attenuation = 1;
      array<float>^ g = Window == nullptr ? y : Window->Processing(y, attenuation);
      pin_ptr<float> px = &(g[0]);
      std::auto_ptr<native::CudaDFourier> dft(new native::CudaDFourier(px, y->Length));
      for (int k = 0; k < y->Length; k++)
      {
        native::Complex coef = dft->Harmonic(k) / attenuation;
        coefs[k].Re = coef.real();
        coefs[k].Im = coef.imag();
      }
      return coefs;
    }
    virtual array<float>^ Inverse(array<Complex>^ c)
    {
      return nullptr;
    }

    virtual void StartTransform(array<float>^ y, int threads)
    {
      FCoefs = gcnew array<Complex>(y->Length);
      FCount = 0;
      thread = gcnew Thread(gcnew ParameterizedThreadStart(this, &CudaDFourier::Run));
      thread->Start(y);
    }
    property bool IsDone
    {
      virtual bool get() { return thread->ThreadState == ThreadState::Stopped; }
    }
    property int Count
    {
      virtual int get() { return FCount; }
    }
    property array<Complex>^ Coefs
    {
      virtual array<Complex>^ get() { return FCoefs; }
    }
  private:
    void Run(Object^ obj)
    {
      array<float>^ x = safe_cast< array<float>^ >(obj);
      float attenuation = 1;
      array<float>^ g = Window == nullptr ? x : Window->Processing(x, attenuation);
      pin_ptr<float> px = &(g[0]);
      std::auto_ptr<native::CudaDFourier> dft(new native::CudaDFourier(px, x->Length));
      for (int k = 0; k < FCoefs->Length; k++, FCount++)
      {
        native::Complex coef = dft->Harmonic(k) / attenuation;
        FCoefs[k].Re = coef.real();
        FCoefs[k].Im = coef.imag();
      }
    }
    Thread^ thread;
    array<Complex>^ FCoefs;
    int FCount;
    IWindow^ Window;
	};

  public ref class CudaOrderScannerBase abstract : IScanner
  {
  public:
    CudaOrderScannerBase()
      : cuda_scanner(NULL)
    {
    }
    ~CudaOrderScannerBase()
    {
      delete cuda_scanner;
    }
    property String^ Name { virtual String^ get() = 0; }
    virtual statistic Scan(array<Quote>^ quotes, int timeout, float takeprofit, float stoploss) = 0;
  protected:
    void PrepareScanner(array<Quote>^ quotes, int timeout)
    {
      if (!cuda_scanner)
      {
        std::vector<native::quote> nat_quotes(quotes->Length);
        for (int i = 0; i < quotes->Length; i++)
        {
          nat_quotes[i].open = quotes[i].open;
          nat_quotes[i].high = quotes[i].high;
          nat_quotes[i].low = quotes[i].low;
          nat_quotes[i].close = quotes[i].close;
        }
        cuda_scanner = new native::CudaOrderScan(nat_quotes.data(), nat_quotes.size(), timeout);
      }
    }
    native::CudaOrderScan* cuda_scanner;
  };

  public ref class CudaOrderScannerBuy : CudaOrderScannerBase
  {
  public:
    CudaOrderScannerBuy() : CudaOrderScannerBase() {}
    property String^ Name { virtual String^ get() override { return "BUY"; } }
    virtual statistic Scan(array<Quote>^ quotes, int timeout, float takeprofit, float stoploss) override
    {
      PrepareScanner(quotes, timeout);
      native::statistic s = cuda_scanner->Scan(takeprofit, stoploss, &native::CudaOrderScan::ScanBuy);
      return statistic( statistic::statistic_base(s.profit.avg_delta, s.profit.avg_wait, s.profit.count),
                        statistic::statistic_base(s.loss.avg_delta, s.loss.avg_wait, s.loss.count),
                        statistic::statistic_base(s.timeout.avg_delta, s.timeout.avg_wait, s.timeout.count),
                        s.wcount);
    }
  };

  public ref class CudaOrderScannerSell : CudaOrderScannerBase
  {
  public:
    CudaOrderScannerSell() : CudaOrderScannerBase() {}
    property String^ Name { virtual String^ get() override { return "SELL"; } }
    virtual statistic Scan(array<Quote>^ quotes, int timeout, float takeprofit, float stoploss) override
    {
      PrepareScanner(quotes, timeout);
      native::statistic s = cuda_scanner->Scan(takeprofit, stoploss, &native::CudaOrderScan::ScanSell);
      return statistic( statistic::statistic_base(s.profit.avg_delta, s.profit.avg_wait, s.profit.count),
                        statistic::statistic_base(s.loss.avg_delta, s.loss.avg_wait, s.loss.count),
                        statistic::statistic_base(s.timeout.avg_delta, s.timeout.avg_wait, s.timeout.count),
                        s.wcount);
    }
  };
}
