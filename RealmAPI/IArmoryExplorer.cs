﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace RealmAPI
{
    public interface IArmoryExplorer : IDisposable
    {
        WebClient WebClient { get; set; }
    }
}
