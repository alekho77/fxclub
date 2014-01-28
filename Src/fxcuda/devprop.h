#pragma once
#include <memory>
#include <string>

namespace native
{
    int GetDeviceCount();
    int CurrentDevice();

    struct DeviceProp
    {
        std::string Name;
        long long GlobalMem;
        long long SharedMem;
        long long ConstMem;
        int ThreadsPerBlock;
        int ThreadsDim[3];
        int GridSize[3];
        int ClockRate;
        int Major;
        int Minor;
        int ProcessorCount;
        int CurrentKernels;
        int MemoryClockRate;
        int ThreadsPerProcessor;
    };

    std::tr1::shared_ptr<DeviceProp> GetDeviceProp(int dev);
}
