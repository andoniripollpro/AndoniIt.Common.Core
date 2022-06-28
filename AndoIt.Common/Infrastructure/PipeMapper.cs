using AndoIt.Common.Interface;
using Newtonsoft.Json;
using System;

namespace AndoIt.Common.Infrastructure
{
    public class PipeMapper <T1, T2>
    {
        public IMapper<T1, T2> Mapper { get; private set; }
        public IDispenser<T1> Dispenser { get; private set; }
        public IProcesser<T2> Processer { get; private set; }
        public ILog log;

        public PipeMapper(IMapper<T1, T2> mapper, IDispenser<T1> dispenser, IProcesser<T2> processer, ILog log = null)
        {
            this.Mapper = mapper ?? throw new ArgumentNullException("mapper");
            this.Dispenser = dispenser ?? throw new ArgumentNullException("dispenser");
            this.Processer = processer ?? throw new ArgumentNullException("listener");
            this.log = log; // If null = Ok

            this.Dispenser.Dispense += this.Dispensed;
        }

        private void Dispensed(object sender, T1 toBeDispensed)
        {
            this.log?.Info($"To be mapped = '{JsonConvert.SerializeObject(toBeDispensed)}'");
            //  Do mapping
            T2 mappedObject = this.Mapper.Map(toBeDispensed);
            this.log?.Info($"Mapped = '{JsonConvert.SerializeObject(mappedObject)}'");

            //  Send to prcess
            this.Processer.Process(mappedObject);
        }
    }
}
