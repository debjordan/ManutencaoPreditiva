using System;

namespace ManutencaoPreditiva.Domain.Exceptions
{
    public class MachineNotFoundException : Exception
    {
        public MachineNotFoundException(string message) : base(message) { }
    }
}
