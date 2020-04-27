using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
    [DataContract(Namespace = "http://server/MobiusServices/v1.0")]
    public class DictionaryMx
    {
        [DataMember] public string Name; // dictionary name
        [DataMember] public List<string> Words = null; // ordered list of words in this dictionary
        [DataMember] public Dictionary<string, string> WordLookup; // definitions in this dictionary looked up by word
        [DataMember] public Dictionary<string, string> DefinitionLookup; // words in this dictionary looked up by definition
    }
}
