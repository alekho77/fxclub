using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FxCuda;

namespace fxanalysis
{
    class GPUTest : ICommand
    {
        public bool Execute(IList<string> cmd_params)
        {
            if (cmd_params.Count == 0)
            {
                Console.WriteLine(" Found {0} CUDA devices", Cuda.DeviceCount);
                if (Cuda.DeviceCount > 0)
                {
                    DeviceProp prop = Cuda.GetDeviceProp(Cuda.CurrentDevice);
                    Console.WriteLine(" Current device is [{0}]: {1} ver{2}.{3}", Cuda.CurrentDevice, prop.Name, prop.Major, prop.Minor);
                    Console.WriteLine(" Device memory Global/Shared/Const: {0}MB/{1}KB/{2}KB ({3} MHz)", prop.GlobalMem / (1024 * 1024), prop.SharedMem / 1024, prop.ConstMem / 1024, prop.MemoryClockRate / 1000);
                    Console.WriteLine(" Device has {0} multiprocessors and {1} kernels ({2} MHz)", prop.ProcessorCount, prop.CurrentKernels, prop.ClockRate / 1000);
                    Console.WriteLine(" Maximum size [{0}x{1}x{2}] of grid and [{3}x{4}x{5}] threads per block", prop.GridSize[0], prop.GridSize[1], prop.GridSize[2], prop.ThreadsDim[0], prop.ThreadsDim[1], prop.ThreadsDim[2]);
                    Console.WriteLine(" Maximum number of threads per block: {0}", prop.ThreadsPerBlock);
                    Console.WriteLine(" Maximum resident threads per multiprocessor: {0}", prop.ThreadsPerProcessor);
                }
                return true;
            }
            return false;
        }
    }
}
