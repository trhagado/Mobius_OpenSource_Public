using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Services.Types.Internal
{
    public class UserObjectUpdate
    {
        public UserObjectUpdateType UpdateType;
        public UserObjectNode AffectedUserObjectNode;
    }

    public enum UserObjectUpdateType
    {
        Create,
        Update,
        Delete
    }
}
