﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNS.Authentication.OAuth2
{
    public class ResourceOwner
    {
        public String Name { get; private set; }

        internal ResourceOwner(String name)
        {
            Name = name;
        }
    }
}
