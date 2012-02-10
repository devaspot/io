using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.Reflection.Emit;

namespace io
{
	public class IoCLI {

        static void Main()
        {
            IoState state = new IoState();
            state.prompt(state);

        }

    }
}
