#pragma once
#include <cuda.h>

struct dim3;

namespace native
{
  enum
  {
    ThreadX = 16,
    ThreadY = 16,
    ThreadCount = ThreadX * ThreadY,
    GridXmax = 32768
  };

  struct CudaDim
  {
    CudaDim(unsigned long long count);
    ~CudaDim();
    int Nblocks() const;
    dim3* gridSize;
    dim3* blockSize;
  };

  template <typename T> 
  T* CudaCreater(size_t n)
  {
    T* ptr;
    cudaMalloc(&ptr, sizeof(T) * n);
    return ptr;
  }

  template <typename T>
  void CudaDeleter(T* ptr)
  {
    if (ptr) cudaFree(ptr);
  }
}
