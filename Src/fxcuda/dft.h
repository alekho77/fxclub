#pragma once
#include <memory>
#include <complex>
#include <vector>
#include "cubase.h"

namespace native
{
  typedef std::complex<float> Complex;

  class CudaDFourier
  {
  public:
    CudaDFourier(float* x, int n);
    Complex Harmonic(int k);
  private:
    const int N;
    std::tr1::shared_ptr<float> devX;
    std::tr1::shared_ptr<float> devTurners;
    CudaDim cuda_dim;
  };
}
