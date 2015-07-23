/**
 *  Insteon Dimmable Switch
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
 
preferences {       
      
}

metadata {
	definition (name: "Insteon Dimmable Switch", namespace: "bitbounce", author: "Keith S") {
    	capability "Actuator"
		capability "Switch"		
		capability "Refresh"   
        capability "switchLevel"
        
        command rampOn
        command rampOff
	}

	simulator {		
	}

	tiles {
		standardTile("switch", "device.switch", width: 2, height: 2, canChangeIcon: false) {
			state "on", label: 'Fast off', action: "switch.off", icon: "st.switches.light.on", backgroundColor: "#79b821"
			state "off", label: 'Fast on', action: "switch.on", icon: "st.switches.light.off", backgroundColor: "#ffffff"
		}	
        standardTile("ramp", "device.switch", width: 1, height: 1, canChangeIcon: false) {
			state "on", label: 'Ramp off', action: "rampOff", icon: "st.switches.light.on", backgroundColor: "#0066CC"
			state "off", label: 'Ramp on', action: "rampOn", icon: "st.switches.light.off", backgroundColor: "#99CCFF"
		}	        
        controlTile("levelSliderControl", "device.level", "slider", height: 1, width: 3, inactiveLabel: false, range:"(0..255)") {
			state "level", action:"switch level.setLevel" 
		}
		standardTile("refresh", "device.refresh", inactiveLabel: false, decoration: "flat") {
			state "default", label:'', action:"refresh.refresh", icon:"st.secondary.refresh"
		}

		main "switch"
		details(["switch","ramp","refresh","levelSliderControl"])
	}
}

def update(String status, level )
{	
  sendEvent (name: "switch", value: "${status}")
  sendEvent (name: "level", value: level)
}

def setDimmer(state, fast)
{
	def level = device.currentState("level")?.value
    log.trace "dimmer: state ${state}, level ${level}, fast ${fast}"
    parent.setDimmerState(this, state, level, fast)
    sendEvent (name: "switch", value: "${state}")
}

def rampOn()
{
	log.debug "ramp on"
    setDimmer("on", 0)
}

def on() {
	log.debug "on"
	setDimmer("on", 1)    
}

def rampOff()
{
	log.debug "ramp off"
    setDimmer("off", 0)
}

def off() {
	log.debug "off"
	setDimmer("off", 1)
}

def setLevel(value) 
{
	sendEvent(name: "level", value: value)
	def state = device.currentState("switch")?.value
    
    if (state == "on")
    {
    	log.trace "setLevel: ${value}"
    	parent.setDimmerState(this, 1, value, 0)
    }
}

def refresh() {
	log.debug "Not Implemented"
}