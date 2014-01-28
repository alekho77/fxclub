#pragma unmanaged
#include "cubase.h"
#include <vector_types.h>

namespace native
{
  CudaDim::CudaDim( unsigned long long count )
  {
    if (count <= ThreadCount)
    {
      gridSize = new dim3(1, 1);
      blockSize = new dim3( ThreadX, (unsigned)((count + ThreadX - 1) / ThreadX) );
    }
    else
    {
      const unsigned long long nblocks = (count + ThreadCount - 1) / ThreadCount;
      unsigned grid_minor = 1; // найдем его как кв. корень от nblocks
      for (long long i = 1, nfrac = nblocks; nfrac >= i; i += 2)
      {
        nfrac -= i;
        grid_minor++;
      }
      unsigned grid_major = (unsigned)((nblocks + grid_minor - 1) / grid_minor);
      gridSize = new dim3(grid_minor, grid_major);
      blockSize = new dim3(ThreadX, ThreadY);
    }
  }

  CudaDim::~CudaDim()
  {
    delete gridSize;
    delete blockSize;
  }

  int CudaDim::Nblocks() const
  {
    return gridSize->x * gridSize->y;
  }

}
