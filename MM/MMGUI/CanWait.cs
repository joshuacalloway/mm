using RealTick.Api.Application;
using RealTick.Api.ClientAdapter;
using RealTick.Api.Domain.Livequote;
using RealTick.Api.Domain.Order;
using RealTick.Api.Domain.Ticks;
using RealTick.Api.Domain;
using RealTick.Api.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System;
using RealTick.Api.Domain.Regional;
using RealTick.Api.Data;


namespace mm {


  public abstract class CanWait {

    protected abstract void WriteLine(string fmt, params object[] args);

    protected bool WaitAny(int millisecondsTimeout, params System.Threading.WaitHandle[] handles)
    {
      // We are basically just calling System.Threading.WaitHandle.WaitAny on the handle(s) provided,
      // but we also include the example's _stopHandle in the list of handles;  this is an event that
      // gets fired when the user clicks the "Stop" button, allowing us to have a more responsive GUI.
      // In a command-line version of the same example, you would leave that part out.

      System.Threading.WaitHandle[] ar = new System.Threading.WaitHandle[handles.Length + 1];
      ar[0] = _stopEvent;
      for (int i = 0; i < handles.Length; i++)
	ar[i + 1] = handles[i];

      int n = System.Threading.WaitHandle.WaitAny(ar, millisecondsTimeout);
      if (n == System.Threading.WaitHandle.WaitTimeout)
	{
	  WriteLine("TIMED OUT WAITING FOR A RESPONSE");
	  return false;
	}
      if (n == 0)
	{
	  WriteLine("CANCELLED BY USER");
	  return false;
	}
      return true;
    }

    System.Threading.AutoResetEvent _stopEvent = new System.Threading.AutoResetEvent(false);

  }


}