//
// BaseVsaEngine.cs: 
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;
using System.Reflection;
using System.Security.Policy;
using System.Threading;

namespace Microsoft.Vsa {

	public abstract class BaseVsaEngine : IVsaEngine {
		
		private const int ROOT_MONIKER_MAX_SIZE = 256;
		private bool monikerAlreadySet;
		private string rootMoniker;

		private bool closed;
		private bool busy;
		private bool empty;
		private bool siteAlreadySet;
		private bool running;

		// indicates that RootMoniker and Site have been set.
		private bool initialized;

		private bool namespaceNotSet;
		private bool supportDebug;
		private bool generateDebugInfo;
		private bool compiled;
		private bool dirty;

		private bool initNewCalled;

		private IVsaSite site;
		private IVsaItems items;

		// its default value has to be "JScript"
		private string language;

		private string version;
		private int lcid;
		private Assembly assembly;
		private Evidence evidence;
		private string name;
		private string rootNamespace;

		public BaseVsaEngine (string language, string version, bool supportDebug)
		{
			this.language = language;

			// FIXME: I think we must ensure that version it's 
			// compliant with versionformat Major.Minor.Build.Revision. 
			// Not sure about what Exception throw.
			this.version = version;

			this.supportDebug = supportDebug;
			this.site = null;
			this.rootMoniker = "";
			this.running = false;
			this.evidence = null;
			this.compiled = false;
			this.dirty = false;

			this.lcid = Thread.CurrentThread.CurrentCulture.LCID;
			this.name = "";
			
			this.rootNamespace = "";
			this.namespaceNotSet = true;	

			this.initialized = false;
			this.initNewCalled = false;
			this.generateDebugInfo = false;
			this.closed = false;
			this.items = null;
			this.siteAlreadySet = false;
		}

		public System._AppDomain AppDomain {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		// FIXME: research if we can use "get" Evidence when: running and busy.
		public Evidence Evidence {
			get { 
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);

				return evidence; 
			}

			set {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (running)
					throw new VsaException (VsaError.EngineRunning);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);

				evidence = value;
			}
		}

		public string ApplicationBase {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public Assembly Assembly {
			get {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (!running)
					throw new VsaException (VsaError.EngineNotRunning);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);

				return assembly;
			}
		}


		// FIXME: research if we can "get" it when running and busy.
		// FIXME: when do we throw VsaException with 
		//        VsaError set to DebugInfoNotSupported?

		public bool GenerateDebugInfo {
			get {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);

				return generateDebugInfo;
			}

			set {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (running)
					throw new VsaException (VsaError.EngineRunning);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);
				else if (!supportDebug)
					throw new VsaException (VsaError.DebugInfoNotSupported);

				generateDebugInfo = value;
			}
		}

		public bool IsCompiled {
			get {				
				if (dirty)
					return false;
				else if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);

				return compiled;
			}
		}


		// FIXME: Research if we can "set" it when running
		public bool IsDirty {
			set {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);

				dirty = value;
			}

			get {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);

				return dirty;
			}
		}

		public bool IsRunning {
			get {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);
				
				return running;
			 }
		}

		public IVsaItems Items {
			get {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);

				return items;
			}
		}


		public string Language {
			get {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);

				return language;
			}
		}

		// FIXME: research when LCIDNotSupported gets thrown.
		public int LCID {
			get { 
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);
				else if (running)
					throw new VsaException (VsaError.EngineRunning);

				return lcid; 
			}

			set { 
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);
				else if (running)
					throw new VsaException (VsaError.EngineRunning);

				lcid = value;
			}
		}


		// FIXME: we must throw VsaException, VsaError set to EngineNameInUse
		// when setting and name is already in use.
		public string Name {
			get {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);
				else if (running)
					throw new VsaException (VsaError.EngineRunning);

				return name;
			}

			set {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);
				else if (running)
					throw new VsaException (VsaError.EngineRunning);

				name = value;
			}
		}

		// FIXME: We have to check - when setting - that the moniker 
		// is not already in use.	    
		public string RootMoniker {
			get { 
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);

				return rootMoniker; 
			}

			set {
				if (monikerAlreadySet)
					throw new VsaException (VsaError.RootMonikerAlreadySet);
				else if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else {
					MonikerState state = ValidateRootMoniker (value);

					switch (state) {
					case MonikerState.Valid:
						rootMoniker = value;
						monikerAlreadySet = true;
						break;

					case MonikerState.Invalid:
						throw new VsaException (VsaError.RootMonikerInvalid);

					case MonikerState.ProtocolInvalid:
						throw new VsaException (VsaError.RootMonikerProtocolInvalid);
					}
				}
			}	
		}

		internal static MonikerState ValidateRootMoniker (string n)
		{
			if (n == null || n == "" || n.Length > ROOT_MONIKER_MAX_SIZE)
				return MonikerState.Invalid;

			try {
				Uri uri = new Uri (n);
				string protocol = uri.Scheme;

				if (protocol == "http" || protocol == "file" || 
				    protocol == "ftp" || protocol == "gopher" || 
				    protocol == "https" || protocol == "mailto")
					return MonikerState.ProtocolInvalid;

				return MonikerState.Valid;

			} catch (UriFormatException e) {
				return MonikerState.Invalid;
			}
		}


		// FIXME: we must check - when setting - that the value is a valid 
		// namespace (research what does that mean :-)). If not the case
		// VsaException must be thrown, set to VsaError.NamespaceInvalid
		public string RootNamespace {
			get { 
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);
				else if (running)
					throw new VsaException (VsaError.EngineRunning);

				return rootNamespace; 
			}

			set {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);
				else if (running)
					throw new VsaException (VsaError.EngineRunning);
				
				rootNamespace = value;
				namespaceNotSet = false;				
			}
		}

		public IVsaSite Site {
			get {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!monikerAlreadySet)
					throw new VsaException (VsaError.RootMonikerNotSet);

				return site;
			}

			set {
				if (!monikerAlreadySet)
					throw new VsaException (VsaError.RootMonikerNotSet);
				else if (siteAlreadySet)
					throw new VsaException (VsaError.SiteAlreadySet);
				else if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (value == null)
					throw new VsaException (VsaError.SiteInvalid);

				site = value;
				siteAlreadySet = true;
			}
		}

		//
		// Version Format: Major.Minor.Revision.Build
		//
		public string Version {
			get {
				if (closed)
					throw new VsaException (VsaError.EngineClosed);
				else if (busy)
					throw new VsaException (VsaError.EngineBusy);
				else if (!initNewCalled)
					throw new VsaException (VsaError.EngineNotInitialized);

				return version;
			}
		}

		public virtual void Close ()
		{
			running = false;
		}

		public virtual bool Compile ()
		{
			if (closed)
				throw new VsaException (VsaError.EngineClosed);
			else if (busy)
				throw new VsaException (VsaError.EngineBusy);
			else if (empty)
				throw new VsaException (VsaError.EngineEmpty);
			else if (running)
				throw new VsaException (VsaError.EngineRunning);
			else if (!initNewCalled)
				throw new VsaException (VsaError.EngineNotInitialized);
			else if (namespaceNotSet)
				throw new VsaException (VsaError.RootNamespaceNotSet);

			return false;
		}

		public virtual object GetOption (string name)
		{
			throw new NotImplementedException ();
		}

		public virtual void InitNew ()
		{
			if (closed)
				throw new VsaException (VsaError.EngineClosed);
			else if (busy)
				throw new VsaException (VsaError.EngineBusy);
			else if (initNewCalled)
				throw new VsaException (VsaError.EngineInitialized);
			else if (!monikerAlreadySet)
				throw new VsaException (VsaError.RootMonikerNotSet);
			else if (!siteAlreadySet)
				throw new VsaException (VsaError.SiteNotSet);

			initNewCalled = true;
		}

		public virtual void LoadSourceState (IVsaPersistSite site)
		{
			throw new NotImplementedException ();
		}

		public virtual void Reset ()
		{
			if (closed)
				throw new VsaException (VsaError.EngineClosed);
			else if (busy)
				throw new VsaException (VsaError.EngineBusy);
			else if (!running)
				throw new VsaException (VsaError.EngineNotRunning);

			running = false;
			assembly = null;
		}

		public virtual void RevokeCache ()
		{
			throw new NotImplementedException ();
		}

		public virtual void Run ()
		{
			if (closed)
				throw new VsaException (VsaError.EngineClosed);
			else if (busy)
				throw new VsaException (VsaError.EngineBusy);
			else if (running)
				throw new VsaException (VsaError.EngineRunning);
			else if (!monikerAlreadySet)
				throw new VsaException (VsaError.RootMonikerNotSet);
			else if (!siteAlreadySet)
				throw new VsaException (VsaError.SiteNotSet);
			else if (namespaceNotSet)
				throw new VsaException (VsaError.RootNamespaceNotSet);

			running = true;
		}

		public virtual void SetOption (string name, object value)
		{
			dirty = true;
		}

		public virtual void SaveCompiledState (out byte [] pe, out byte [] debugInfo)
		{
			throw new NotImplementedException ();
		}

		public virtual void SaveSourceState (IVsaPersistSite site)
		{
			throw new NotImplementedException ();
		}

		public abstract bool IsValidIdentifier (string ident);
	}

	public class BaseVsaSite : IVsaSite {

		public virtual byte [] Assembly {
			get { throw new NotImplementedException (); }
		}

		public virtual byte [] DebugInfo {
			get { throw new NotImplementedException (); }
		}

		public virtual void GetCompiledState (out byte [] pe, out byte [] debugInfo)
		{
			throw new NotImplementedException ();
		}

		public virtual object GetEventSourceInstance (string itemName, string eventSourceName)
		{
			throw new NotImplementedException ();
		}

		public virtual object GetGlobalInstance (string name)
		{
			throw new NotImplementedException ();
		}

		public virtual void Notify (string notify, object optional)
		{
			throw new NotImplementedException ();
		}

		public virtual bool OnCompilerError (IVsaError error)
		{
			throw new NotImplementedException ();
		}
	}


	public abstract class BaseVsaStartup {

		public void SetSite (IVsaSite site)
		{
			throw new NotImplementedException ();
		}

		public abstract void Startup ();
		
		public abstract void Shutdown ();
	}

	internal enum MonikerState {
		Valid,
		Invalid,
		ProtocolInvalid
	}		
}
