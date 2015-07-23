/**
 *  Insteon Switch
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
	definition (name: "Insteon Switch", namespace: "bitbounce", author: "Keith S") {
    	capability "Actuator"
		capability "Switch"		
		capability "Refresh"        
	}

	simulator {		
	}

	tiles {
		standardTile("switch", "device.switch", width: 2, height: 2, canChangeIcon: true) {
			state "on", label: '${name}', action: "switch.off", icon: "st.switches.switch.on", backgroundColor: "#79b821"
			state "off", label: '${name}', action: "switch.on", icon: "st.switches.switch.off", backgroundColor: "#ffffff"
		}	
		standardTile("refresh", "device.refresh", inactiveLabel: false, decoration: "flat") {
			state "default", label:'', action:"refresh.refresh", icon:"st.secondary.refresh"
		}

		main "switch"
		details(["switch","refresh"])
	}
}

def update(String status)
{	
  sendEvent (name: "switch", value: "${status}")
}

// handle commands
def on() {
	parent.setSwitchState(this, 1)
    sendEvent (name: "switch", value: "on")
}

def off() {
	parent.setSwitchState(this, 0)
    sendEvent (name: "switch", value: "off")
}

def refresh() {
	log.debug "Not Implemented"
}
