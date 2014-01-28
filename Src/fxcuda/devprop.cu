#pragma unmanaged
#include "devprop.h"

namespace native
{
    int GetDeviceCount()
    {
        int count;
        if (cudaGetDeviceCount(&count) == cudaSuccess)
        {
            return count;
        }
        return -1;
    }

    int CurrentDevice()
    {
        int dev;
        if (cudaGetDevice(&dev) == cudaSuccess)
        {
            return dev;
        }
        return -1;
    }

    std::tr1::shared_ptr<DeviceProp> GetDeviceProp( int dev )
    {
        std::tr1::shared_ptr<DeviceProp> dev_prop;
        cudaDeviceProp prop;
        if(cudaGetDeviceProperties(&prop, dev) == cudaSuccess)
        {
            dev_prop.reset(new DeviceProp);
            dev_prop->Name = prop.name;
            dev_prop->GlobalMem = prop.totalGlobalMem;
            dev_prop->SharedMem = prop.sharedMemPerBlock;
            dev_prop->ConstMem = prop.totalConstMem;
            dev_prop->ThreadsPerBlock = prop.maxThreadsPerBlock;
            memcpy(dev_prop->ThreadsDim, prop.maxThreadsDim, sizeof(prop.maxThreadsDim));
            memcpy(dev_prop->GridSize, prop.maxGridSize, sizeof(prop.maxGridSize));
            dev_prop->ClockRate = prop.clockRate;
            dev_prop->Major = prop.major;
            dev_prop->Minor = prop.minor;
            dev_prop->ProcessorCount = prop.multiProcessorCount;
            dev_prop->CurrentKernels = prop.concurrentKernels;
            dev_prop->MemoryClockRate = prop.memoryClockRate;
            dev_prop->ThreadsPerProcessor = prop.maxThreadsPerMultiProcessor;
        }
        return dev_prop;
    }
}
