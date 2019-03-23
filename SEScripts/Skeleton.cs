using System;
using Sandbox.ModAPI;

public abstract class Skeleton : IMyGridProgram
{
    public Action<string> Echo
    {
        get { throw new NotImplementedException(); }
        set { throw new NotImplementedException(); }
    }

    public TimeSpan ElapsedTime
    {
        get { throw new NotImplementedException(); }
        set { throw new NotImplementedException(); }
    }

    public IMyGridTerminalSystem GridTerminalSystem
    {
        get { throw new NotImplementedException(); }
        set { throw new NotImplementedException(); }
    }

    public bool HasMainMethod
    {
        get { throw new NotImplementedException(); }
    }

    public bool HasSaveMethod
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public IMyProgrammableBlock Me
    {
        get { throw new NotImplementedException(); }
        set { throw new NotImplementedException(); }
    }

    public Sandbox.ModAPI.Ingame.IMyGridProgramRuntimeInfo Runtime
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }

    public string Storage
    {
        get { throw new NotImplementedException(); }
        set { throw new NotImplementedException(); }
    }

    public Func<Sandbox.ModAPI.Ingame.IMyIntergridCommunicationSystem> IGC_ContextGetter { set => throw new NotImplementedException(); }

    Sandbox.ModAPI.Ingame.IMyGridTerminalSystem IMyGridProgram.GridTerminalSystem
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }

    Sandbox.ModAPI.Ingame.IMyProgrammableBlock IMyGridProgram.Me
    {
        get
        {
            throw new NotImplementedException();
        }

        set
        {
            throw new NotImplementedException();
        }
    }

    public void Main(string argument, Sandbox.ModAPI.Ingame.UpdateType updateSource)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    void IMyGridProgram.Main(string argument)
    {
        throw new NotImplementedException();
    }
}