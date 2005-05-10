ifndef TARGET
	TARGET=./bin/Debug
else
	TARGET=./bin/$(TARGET)
endif

MCS=mcs
ifndef (RELEASE)
	MCSFLAGS=-debug 
endif
LIBS=-lib:/usr/lib/mono/1.0 -lib:/usr/lib/mono/gtk-sharp


CHAT_DLL=$(TARGET)/Chat.dll
CHAT_PDB=$(TARGET)/Chat.pdb
CHAT_SRC=Chat/AssemblyInfo.cs \
	Chat/ClientSession.cs \
	Chat/ClientSessions.cs \
	Chat/Server.cs
CHAT_RES=

WINDOWSSERVICESMANAGEMENT_DLL=$(TARGET)/WindowsServicesManagement.dll
WINDOWSSERVICESMANAGEMENT_PDB=$(TARGET)/WindowsServicesManagement.pdb
WINDOWSSERVICESMANAGEMENT_SRC=WindowsServicesManagement/AssemblyInfo.cs \
	WindowsServicesManagement/WindowsServicesManagement.cs
WINDOWSSERVICESMANAGEMENT_RES=

SERVERCONSOLE_EXE=$(TARGET)/ServerConsole.exe
SERVERCONSOLE_PDB=$(TARGET)/ServerConsole.exe
SERVERCONSOLE_SRC=ServerConsole/AssemblyInfo.cs \
	ServerConsole/ServerConsole.cs
SERVERCONSOLE_RES=

SERVERSERVICES_DLL=$(TARGET)/ServerServices.dll
SERVERSERVICES_PDB=$(TARGET)/ServerServices.pdb
SERVERSERVICES_SRC=ServerServices/AppDomainPluginCollection.cs \
	ServerServices/AssemblyInfo.cs \
	ServerServices/IONotificationQueue.cs \
	ServerServices/PluginAppDomain.cs \
	ServerServices/PluginAppDomainCollection.cs \
	ServerServices/PluginFinder.cs \
	ServerServices/PluginManager.cs \
	ServerServices/PortManager.cs \
	ServerServices/PortSocketMap.cs \
	ServerServices/Services.cs
SERVERSERVICES_RES=

STATICWEB_DLL=$(TARGET)/StaticWeb.dll
STATICWEB_PDB=$(TARGET)/StaticWeb.pdb
STATICWEB_SRC=StaticWeb/AssemblyInfo.cs \
	StaticWeb/StaticWeb.cs
STATICWEB_RES=

PLUGINSERVICES_DLL=$(TARGET)/PluginServices.dll
PLUGINSERVICES_PDB=$(TARGET)/PluginServices.pdb
PLUGINSERVICES_SRC=PlugInServices/AssemblyInfo.cs \
	PlugInServices/IFilter.cs \
	PlugInServices/IHandler.cs \
	PlugInServices/IPlugin.cs \
	PlugInServices/IServer.cs
PLUGINSERVICES_RES=

all: \
$(CHAT_DLL)\
$(SERVERCONSOLE_EXE)\
$(SERVERSERVICES_DLL)\
$(STATICWEB_DLL)\
$(PLUGINSERVICES_DLL)
	-mkdir -p $(TARGET)/../plugins
	-cp $(STATICWEB_DLL) $(CHAT_DLL) $(TARGET)/../plugins

$(CHAT_DLL): $(CHAT_SRC) $(PLUGINSERVICES_DLL)
	-mkdir -p $(TARGET)
	$(MCS) $(MCSFLAGS) $(LIBS) -r:System.dll -r:System.Data.dll -r:System.Xml.dll -r:$(PLUGINSERVICES_DLL) -target:library -out:$(CHAT_DLL) $(CHAT_RES) $(CHAT_SRC)

$(WINDOWSSERVICESMANAGEMENT_DLL): $(WINDOWSSERVICESMANAGEMENT_SRC) $(PLUGINSERVICES_DLL)
	-mkdir -p $(TARGET)
	$(MCS) $(MCSFLAGS) $(LIBS) -r:System.dll -r:$(PLUGINSERVICES_DLL) -r:System.ServiceProcess.dll -r:System.Web.dll -r:System.Windows.Forms.dll -target:library -out:$(WINDOWSSERVICESMANAGEMENT_DLL) $(WINDOWSSERVICESMANAGEMENT_RES) $(WINDOWSSERVICESMANAGEMENT_SRC)

$(SERVERCONSOLE_EXE): $(SERVERCONSOLE_SRC) $(SERVERSERVICES_DLL) $(PLUGINSERVICES_DLL)
	-mkdir -p $(TARGET)
	$(MCS) $(MCSFLAGS) $(LIBS) -r:System.dll -r:System.Data.dll -r:System.Xml.dll -r:$(SERVERSERVICES_DLL) -r:$(PLUGINSERVICES_DLL) -target:exe -out:$(SERVERCONSOLE_EXE) $(SERVERCONSOLE_RES) $(SERVERCONSOLE_SRC)
	cp ServerConsole/App.config $(SERVERCONSOLE_EXE).config

$(SERVERSERVICES_DLL): $(SERVERSERVICES_SRC) $(PLUGINSERVICES_DLL)
	-mkdir -p $(TARGET)
	$(MCS) $(MCSFLAGS) $(LIBS) -r:System.dll -r:System.Data.dll -r:System.Xml.dll -r:$(PLUGINSERVICES_DLL) -target:library -out:$(SERVERSERVICES_DLL) $(SERVERSERVICES_RES) $(SERVERSERVICES_SRC)

$(STATICWEB_DLL): $(STATICWEB_SRC) $(PLUGINSERVICES_DLL)
	-mkdir -p $(TARGET)
	$(MCS) $(MCSFLAGS) $(LIBS) -r:System.dll -r:System.Data.dll -r:System.Xml.dll -r:$(PLUGINSERVICES_DLL) -target:library -out:$(STATICWEB_DLL) $(STATICWEB_RES) $(STATICWEB_SRC)

$(PLUGINSERVICES_DLL): $(PLUGINSERVICES_SRC) 
	-mkdir -p $(TARGET)
	$(MCS) $(MCSFLAGS) $(LIBS) -r:System.dll -r:System.Data.dll -r:System.Xml.dll -target:library -out:$(PLUGINSERVICES_DLL) $(PLUGINSERVICES_RES) $(PLUGINSERVICES_SRC)


# common targets

all:	$(CHAT_DLL) \
	$(SERVERCONSOLE_EXE) \
	$(SERVERSERVICES_DLL) \
	$(STATICWEB_DLL) \
	$(PLUGINSERVICES_DLL)

clean:
	-rm -f "$(CHAT_DLL)" 2> /dev/null
	-rm -f "$(CHAT_PDB)" 2> /dev/null
	-rm -f "$(WINDOWSSERVICESMANAGEMENT_DLL)" 2> /dev/null
	-rm -f "$(WINDOWSSERVICESMANAGEMENT_PDB)" 2> /dev/null
	-rm -f "$(SERVERCONSOLE_EXE)" 2> /dev/null
	-rm -f "$(SERVERCONSOLE_PDB)" 2> /dev/null
	-rm -f "$(SERVERSERVICES_DLL)" 2> /dev/null
	-rm -f "$(SERVERSERVICES_PDB)" 2> /dev/null
	-rm -f "$(STATICWEB_DLL)" 2> /dev/null
	-rm -f "$(STATICWEB_PDB)" 2> /dev/null
	-rm -f "$(PLUGINSERVICES_DLL)" 2> /dev/null
	-rm -f "$(PLUGINSERVICES_PDB)" 2> /dev/null


# project names as targets

Chat: $(CHAT_DLL)
WindowsServicesManagement: $(WINDOWSSERVICESMANAGEMENT_DLL)
ServerConsole: $(SERVERCONSOLE_EXE)
ServerServices: $(SERVERSERVICES_DLL)
StaticWeb: $(STATICWEB_DLL)
PluginServices: $(PLUGINSERVICES_DLL)

