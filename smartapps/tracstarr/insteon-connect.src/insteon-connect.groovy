/**
 *  Insteon
 *
 *  Copyright 2015 Keith S
 *
 *  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 *  in compliance with the License. You may obtain a copy of the License at:
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
 *  on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License
 *  for the specific language governing permissions and limitations under the License.
 *
 */
 
definition(
    name: "Insteon Connect",
    namespace: "tracstarr",
    author: "tracstarr",
    description: "Connect to and use Insteon service controller",
    category: "My Apps",
    iconUrl: "http://wscont1.apps.microsoft.com/winstore/1x/c4c350bb-ff7b-4491-afd6-8d44fdfd5554/Icon.342447.png",
    iconX2Url: "http://wscont1.apps.microsoft.com/winstore/1x/c4c350bb-ff7b-4491-afd6-8d44fdfd5554/Icon.342447.png",
    iconX3Url: "http://wscont1.apps.microsoft.com/winstore/1x/c4c350bb-ff7b-4491-afd6-8d44fdfd5554/Icon.342447.png")

/************************************************************
*	Preferences
*/

preferences {
	
     page(name:"mainPage", title:"Insteon Connect", content: "mainPage") 
     page(name:"settingsPage", title:"Insteon Connect Settings", content: "serverSettingsPage") 
     page(name:"serviceConnectPage", title:"Insteon Service Connect", content:"serviceConnectPage", refreshTimeout:2, install:true)
     
     page(name:"deviceSelectPage", title:"Device Select", content:"deviceSelectPage")
          
     page(name:"statusCheckPage", content:"statusCheckPage")
     page(name:"resetServerSettingsPage", content:"resetServerSettingsPage")
     page(name:"resetDeviceListPage", content:"resetDeviceListPage")
     page(name:"forceRefreshPage", content:"forceRefreshPage")
          
}

/************************************************************
*	REST Endpoint Definitions
*/
mappings {
    path("/link") {
		action: [
			GET: "linkHandler",
		]
	}    
  	path("/revoke") {
       action: [
           GET: "revokeHandler",
       ]
   	}
   	path("/deviceupdate/:id/:status") {
       action: [
           PUT: "deviceHandler",
       ]
   	}
    path("/dimmerupdate/:id/:status/:level") {
       action: [
           PUT: "dimmerHandler",
       ]
   	}
}

/************************************************************
*	Pages
*/

def mainPage(){	
	return dynamicPage(name:"mainPage", title: "Insteon Connect", uninstall: state.isLinked, install: false) {
    			section() {
                	href ("settingsPage", title: state.isLinked? "Server Settings":"Connect", description:"Insteon server settings.")
                }
                
                if (state.isLinked)
                {
                	UpdateSelectedDevices()
                
                	section() {
                		href ("deviceSelectPage", title: "Devices", description: "Select the devices you want to control.")                      
                    }
                }
                else
                {
                	section() {
                		paragraph ("No link has been established to your Insteon server. Please click Connect.")
                    }
                }
            }

}

def serverSettingsPage(){
	return dynamicPage(name:"settingsPage", title: "Insteon Connect Settings", uninstall:false, install:false){
    		section() {
            	paragraph "Please enter your Insteon Service Endpoint details."
                input("ip", "string", title:"IP Address", description: "IP Address", required: true, defaultValue: "192.168.20.219")
                input("port", "string", title:"Port", description: "Port", defaultValue: 8875 , required: true)
                
                
                if (state.isLinked)
                {
                	href ("statusCheckPage", title: "Status Check", description: "Check if your Insteon Controller is connected and running correctly.")
                    href ("resetServerSettingsPage", title: "Reset", description: "Reset your Insteon server settings and oAuth.")                        
                    href ("resetDeviceListPage", title: "Reset Device List", description: "Refetch devices.")                        
                    href ("forceRefreshPage", title: "Refresh all device statuses", description: "Refresh the status of all devices.")        
                }
                else
                {
                	href ("serviceConnectPage", title:"Link", description:"Try to link to your local insteon server.")
                }
                 
            }            
            
        }
}

def deviceSelectPage(){
	if (state.gotDevices)
    {
        def switchOptions = switchesDiscovered() ?: []
        def numSwitchesFound = switchOptions.size() ?: 0
        
        def dimmerOptions = dimmersDiscovered() ?: []
        def numDimmersFound = dimmerOptions.size() ?: 0
        
        def motionOptions = motionDiscovered() ?: []
        def numMotionFound = motionOptions.size() ?: 0
     
     	return dynamicPage(name:"deviceSelectPage", title:"Device Selection", nextPage:"") {
			section("Select your device below.") {
        		input "selectedSwitches", "enum", required:false, title:"Select Switches (${numSwitchesFound} found)", multiple:true, options:switchOptions	
                input "selectedDimmers", "enum", required:false, title:"Select Dimmers (${numDimmersFound} found)", multiple:true, options:dimmerOptions	
                input "selectedMotion", "enum", required:false, title:"Select Motion (${numMotionFound} found)", multiple:true, options:motionOptions	
			}		
		}   
    }
    
    if (!state.waitOnRestCall)
    {
		api("deviceList",null)	
    }
    
	return dynamicPage(name:"deviceSelectPage", title:"Device Discovery Started!", nextPage:"", refreshInterval:5) {
		section() {
				paragraph "Getting device lists..."
			}	
	}   
}

def serviceConnectPage(){
	TRACE("${state}")
	if (!state.isLinked)
	{    	
        if (!state.connectingToService)
     	{        	
            insteonHubConnect()
    		TRACE("Waiting for response from Insteon local service.")
            state.connectingToService = true
    	}
        
        return dynamicPage(name:"serviceConnectPage", title:"Connecting to Insteon Service",refreshInterval:3) {
                section() {
                    paragraph "Please Wait"
                }
            }
    }
    else
    {
    	TRACE("Connected to Insteon Service !")
        state.connectingToService = false
        return dynamicPage(name:"serviceConnectPage", title:"Connecting to Insteon Service", install:true, uninstall: false) {
                section() {
                    paragraph "Connected. Please click Done."
                }
            }
    }
}

def resetDeviceListPage (){
	state.gotDevices = false
    state.waitOnRestCall = false
    state.switches = null
    state.dimmers = null
    state.motion = null
    
    return dynamicPage(name:"resetDeviceListPage", title:"Reset Devices") {
			section() {
				paragraph "Reset."
			}
		}
}

def forceRefreshPage(){
	api("fullrefresh",null)
	return dynamicPage(name:"forceRefreshPage", title:"Force Refresh") {
			section() {
				paragraph "Done"
			}
		}
}

def resetServerSettingsPage(){
	return dynamicPage(name:"resetServerSettingsPage", title:"Reset All") {
			section() {
				paragraph "Not Implemented..."
			}
		}
}

def statusCheckPage(){   	
    if (!state.statusCheck)
    {
      api("status",null)
      state.statusCheck = true
      return dynamicPage(name:"statusCheckPage", title:"Status Check", refreshInterval:3) {
			section() {
				paragraph "Getting Status..."
			}
		}
	}
    
    state.statusCheck = false
    
    if (!state.isLinked)
    {
        return dynamicPage(name:"statusCheckPage", title:"Status"){
    		section() {
            	paragraph "Not linked"
            }
        }	
    }
    
    return dynamicPage(name:"statusCheckPage", title:"Status"){
    		section() {
            	paragraph "Connected"
            }
    }
}

/************************************************************
*	Install/Uninstall/Updated
*/
def installed() {
	DEBUG( "Installed with settings: ${settings}")
	initialize()
}

def updated() {
	DEBUG("Updated with settings: ${settings}")

	unsubscribe()
	initialize()
}

def initialize() {
	 DEBUG("Initialize")
     
     subscribe(location, null, lanHandler, [filterEvents:false])        
}

def uninstalled() {
	api("reset",null)
    removeChildDevices(getChildDevices())
}

private removeChildDevices(delete) {
    delete.each {
        deleteChildDevice(it.deviceNetworkId)
    }
}

/************************************************************
*	REST Handlers
*/

def linkHandler(){	
    if (state.isLinked)
    {
    	return [result: "already connected"]
    }
   
   	state.isLinked = true
    state.waitOnRestCall = false
   	return [result  : "ok"]
}

def revokeHandler(){
    INFO("Insteon service requested revoking access")
    state.isLinked = false
    state.waitOnRestCall = false
    return [result  : "ok"]
}

def deviceHandler(){
	updateDevice()
}

def dimmerHandler(){
	updateDimmer()
}

/************************************************************
*	ST subscription Handlers
*/

def lanHandler(evt) {
	
	def description = evt.description
    def hub = evt?.hubId
	
	def parsedEvent = parseEventMessage(description)
	parsedEvent << ["hub":hub]
       
    if (parsedEvent.headers)
    {
    	try
        {	        	
            def headerString = new String(parsedEvent.headers.decodeBase64())
            DEBUG(headerString)
            if (!headerString.contains("Insteon HttpListener"))
            {
                WARN("Not a message from Insteon Services. Ignoring")
                return
            }
            
            def bodyString = new String(parsedEvent.body.decodeBase64())
            def body = new groovy.json.JsonSlurper().parseText(bodyString)
            
            if (body?.errorCode)
            {
                ERROR("[${body?.errorCode}] ${body?.message}")
            }
            else
            {
                if (body?.action?.equalsIgnoreCase("status"))
                {               	
                    state.isLinked = body?.isOk
                }
                else if (body?.level != null)
                {
                	// should be dimmalbe status response
                    DEBUG("dimmable status response")
                    // from insteon docs, level 0 means it's "off", not necessarly the dimmable set level. If off, don't bother changing the dim level in the ui
                    if (body?.level > 0){
                    	updateDimmerCurrentLevel(body?.deviceId, body?.level)
                    }
                }
                else if (body?.devices != null)
                {           
                    body?.devices.each { 

                        def d = null

                        if (it?.category?.equalsIgnoreCase("Dimmable Lighting Control"))
                        {    
                            d = getDimmers()                     
                        }
                        else if (it?.category?.equalsIgnoreCase("Switched Lighting Control"))
                        {       
                            d = getSwitches()                		
                        }
                        else if (it?.subCategory?.equalsIgnoreCase("Motion Sensor [2420M]"))
                        {                        	
                            d = getMotion()
                        }
                        else if (it?.category?.equalsIgnoreCase("Sensors and Actuators"))
                        {
                        	if (it?.subCategory.toLowerCase().contains("iolinc"))
                            {
                            	DEBUG("IOLINC")
                            }
                            else {
                            	DEBUG("not implemented")                                  		
                            }
                        }
                        else
                        {
                            DEBUG("Ignoring current device type. " + it?.category)
                        }

                        if (d != null)
                        {
                            DEBUG("Adding to device list")
                            def dname = it?.name ?: it?.address
                            d[it.address] = [id: it.address, name: dname, category: it?.category, deviceType: it?.subCategory, hub: parsedEvent.hub]   
                        }
                    }

                    state.gotDevices = true
                }             

            }
      	
        } catch(Exception e)
        {
        	ERROR("Error in landHandler:" + e)
        } finally
        {
        	state.waitOnRestCall = false   
        }
    }
}

private def parseEventMessage(String description) {
	def event = [:]
	def parts = description.split(',')
	parts.each { part ->
		part = part.trim()
		if (part.startsWith('devicetype:')) {
			def valueString = part.split(":")[1].trim()
			event.devicetype = valueString
		}
		else if (part.startsWith('mac:')) {
			def valueString = part.split(":")[1].trim()
			if (valueString) {
				event.mac = valueString
			}
		}
		else if (part.startsWith('networkAddress:')) {
			def valueString = part.split(":")[1].trim()
			if (valueString) {
				event.ip = valueString
			}
		}
		else if (part.startsWith('deviceAddress:')) {
			def valueString = part.split(":")[1].trim()
			if (valueString) {
				event.port = valueString
			}
		}		
		else if (part.startsWith('headers')) {
			part -= "headers:"
			def valueString = part.trim()
			if (valueString) {
				event.headers = valueString
			}
		}
		else if (part.startsWith('body')) {
			part -= "body:"
			def valueString = part.trim()
			if (valueString) {
				event.body = valueString
			}
		}
        else if (part.startsWith('requestId')) {
			part -= "requestId:"
			def valueString = part.trim()
			if (valueString) {
				event.requestId = valueString
			}
		}
	}

	event
}
/************************************************************
*	API and http methods
*/

private def getHostAddress() {
    return "${ip}:${port}"
}

private String convertIPtoHex(ipAddress) { 
    String hex = ipAddress.tokenize( '.' ).collect {  String.format( '%02x', it.toInteger() ) }.join()
    return hex
}

private String convertPortToHex(port) {
    String hexport = port.toString().format( '%04x', port.toInteger() )
    return hexport
}

def toJson(Map m){
	return new org.codehaus.groovy.grails.web.json.JSONObject(m).toString()
}

def toQueryString(Map m) {
	return m.collect { k, v -> "${k}=${URLEncoder.encode(v.toString())}" }.sort().join("&")
}

private def api(method, args) {
		
    def methods = [
		'status': 
			[uri:"/status", 
          		type: 'get'],
		'configure': 
			[uri:"/configure", 
          		type: 'put'],
		'reset': 
			[uri:"/configure/reset", 
          		type: 'get'],
        'deviceList': 
			[uri:"/devices", 
          		type: 'get'],
        'setswitch':
        	[uri:"/lighting/switched",
            	type: 'put'],
        'setdimmer':
        	[uri:"/lighting/dimmable",
            	type: 'put'],
        'refreshDevice':
        	[uri:"/refresh/device?" + toQueryString(args),
            	type: 'get'],
        'refreshDimmer':
        	[uri:"/status/lighting/dimmer?" + toQueryString(args),
            	type: 'get'],
        'fullrefresh':
        	[uri:"/forcestatusrefresh",
            	type: 'get'],
		]
        
	def request = methods.getAt(method)
 	
    state.waitOnRestCall = true
    
    try {
		
		if (request.type == 'put') {
			putapi(args,request.uri)
		} else if (request.type == 'get') {
			getapi(request.uri)
		} 
	} catch (Exception e) {
		ERROR("doRequest> " + e)		
	}    	
}

private getapi(uri) {
  
  def hubAction = new physicalgraph.device.HubAction(
    method: "GET",
    path: uri,
    headers: [	HOST:getHostAddress(),
    			"Accept":"application/json"
                ]
  )
  
  sendHubCommand(hubAction)  
}

private putapi(params, uri) {
		
	def hubAction = new physicalgraph.device.HubAction(
		method: "PUT",
		path: uri,
		body: toJson(params),
		headers: [Host:getHostAddress(), "Content-Type":"application/json" ]
		)
	sendHubCommand(hubAction)    
}
/************************************************************
*	Child Device Functions
*/
def setSwitchState(insteonDevice, mode)
{	
	def pId = insteonDevice?.device?.deviceNetworkId
    pId = pId.substring(pId.lastIndexOf("/") + 1)
	def params = ["deviceId": "${pId}","State" : "${mode}"]
    api("setswitch", params)    
}

def setDimmerState(insteonDevice, state, level, fast)
{	
    def pId = insteonDevice?.device?.deviceNetworkId
    pId = pId.substring(pId.lastIndexOf("/") + 1)	
	def params = ["deviceId": "${pId}","State" : "${state}","Fast" : "${fast}", "Level" : "${level}"]
    api("setdimmer", params)    
}

def getDimmerStatus(insteonDevice)
{
	def pId = insteonDevice?.device?.deviceNetworkId
    pId = pId.substring(pId.lastIndexOf("/") + 1)	
	def params = ["deviceId": "${pId}"]
    api("refreshDimmer", params)    
}

/************************************************************
*	Methods
*/
def deleteChildren(selected, existing){
	// given all known devices, search the list of selected ones, if the device isn't selected, see if it exists as a device, if it does, remove it.
    existing.each { device ->
    	def dni = app.id + "/" + device.value.id
        def sel
        
        if (selected)
        {
        	sel = selected.find { dni == it }            
        }
        
        if (!sel)
        {
        	def d = getChildDevice( dni )
            if (d)
            {
            	DEBUG("Deleting device " + dni )
           		deleteChildDevice( dni )
            }        	
        }
    }
}

def DeleteChildDevicesNotSelected() {
	deleteChildren(selectedSwitches, getSwitches())  
    deleteChildren(selectedDimmers, getDimmers()) 
    deleteChildren(selectedMotion, getMotion())
}

def UpdateSelectedDevices() {
	DeleteChildDevicesNotSelected()    
    	
    createNewDevices(selectedSwitches, getSwitches(), "Insteon Switch")   
    createNewDevices(selectedDimmers, getDimmers(), "Insteon Dimmable Switch")   
    createNewDevices(selectedMotion, getMotion(), "Insteon Motion Sensor")
}

private def createNewDevices(selected, existing, deviceType) {
	if (selected)
    {    	
     	selected.each { dni ->
        	def d = getChildDevice(dni)
            if (!d)
            {
            	def newDevice
            	newDevice = existing.find { (app.id + "/" + it.value.id) == dni}
                d = addChildDevice("tracstarr",deviceType, dni, newDevice?.value.hub, [name: newDevice?.value.name])
                DEBUG("Created new " + deviceType)
            }           
        }      
    }
}

def generateAccessToken() {
    
    if (resetOauth) {
    	DEBUG( "Reseting Access Token")
    	state.accessToken = null
        resetOauth = false
    }
    
	if (!state.accessToken) {
    	createAccessToken()
        TRACE( "Creating new Access Token: $state.accessToken")
    }
  
}

private insteonHubConnect() {
	DEBUG("Connecting to Insteon Local Hub")
    
    generateAccessToken()
      
    def params = ["Location": "Home","AppId" : "${app.id}", "AccessToken" : "${state.accessToken}"]
    api("configure", params)
 
}

Map switchesDiscovered() {
	def switches =  getSwitches()
	def map = [:]
	
    switches.each {
        def value = "${it?.value?.name}"
        def key = app.id + "/" + it?.value?.id 
        map["${key}"] = value
    }

	map
}

def getSwitches() {
	state.switches = state.switches ?: [:]
}

Map dimmersDiscovered() {
	def d =  getDimmers()
	def map = [:]
	
    d.each {
        def value = "${it?.value?.name}"
        def key = app.id + "/" + it?.value?.id 
        map["${key}"] = value
    }

	map
}

def getDimmers() {
	state.dimmers = state.dimmers ?: [:]
}


Map motionDiscovered() {
	def devices =  getMotion()
	def map = [:]
	
    devices.each {
        def value = "${it?.value?.name}"
        def key = app.id + "/" + it?.value?.id 
        map["${key}"] = value
    }

	map
}

def getMotion() {
	state.motion = state.motion ?: [:]
}
/************************************************************
*	Insteon Functions
*/
private updateDevice() 
{	
	def insteonId = params.id
    def status = params.status

	TRACE("Update device hit:" + insteonId + ":" + status)
	def childDevice = getChildDevice((app.id + "/" + insteonId))
    
    if (childDevice)
    {    	
    	childDevice.update(status)                
    }    
}

private updateDimmer()
{
	def insteonId = params.id
    def status = params.status
   
	TRACE("Update device hit:" + insteonId + ":" + status)
	def childDevice = getChildDevice((app.id + "/" + insteonId))
    
    if (childDevice)
    {    	
    	childDevice.update(status)                
    }    
}

private updateDimmerCurrentLevel(insteonId, level)
{
	def childDevice = getChildDevice((app.id + "/" + insteonId))
    
    if (childDevice)
    {    	
    	childDevice.setUiLevel(level)                
    }    
}

/************************* DEBUGGING **********************/

private TRACE(message)
{
	log.trace(message)
}

private DEBUG(message)
{
	log.debug(message)
}

private WARN(message)
{
	log.warn(message)
}

private ERROR(message)

{
	log.error(message)
}