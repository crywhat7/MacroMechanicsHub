using System.Collections.Generic;
using System.Windows.Documents;

namespace MacroMechanicsHub.Models
{
    public class AssistantResponse
    {
        public List<string> WhatToDoNext { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> Advices { get; set; }
    }
}
