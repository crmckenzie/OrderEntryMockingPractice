using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OrderEntryMockingPractice.Services
{
    public class InvalidOrderException : Exception
    {
        public ReadOnlyCollection<string> ExceptionMessages { get; private set; }

        public InvalidOrderException(List<string> exceptionMessages) : base(exceptionMessages.Any() ? exceptionMessages[0]: "No errors. Why am I throwing an exception?")
        {
            ExceptionMessages = exceptionMessages.AsReadOnly();
        }
   }
}