/*
 * Insteon Motion Device
 */

metadata {
  definition (name: "Insteon Motion Sensor", author: "tracstarr <tracstarr@hotmail.com>", namespace:"tacstarr") {
    capability "Motion Sensor"
    capability "Sensor"
    capability "Battery"    
    capability "Illuminance Measurement"
  }

  simulator {
  }

  tiles {
    standardTile("motion", "device.motion", width: 2, height: 2, canChangeBackground: true, canChangeIcon: true) {
      state("active",   label:'motion',    icon:"st.motion.motion.active",   backgroundColor:"#53a7c0")
      state("inactive", label:'no motion', icon:"st.motion.motion.inactive", backgroundColor:"#ffffff")      
    }
    
    valueTile("battery", "device.battery", inactiveLabel: false, decoration: "flat") {
			state "battery", label:'${currentValue} battery', unit:""
		}
        
    valueTile("illuminance", "device.illuminance", inactiveLabel: false, decoration: "flat") {
			state "luminosity", label:'${currentValue}', unit:"Light"
		}

    main "motion"
    details(["motion", "battery","illuminance"])
  }
}

// Called from SmartApp
def update(String state) {

  //TODO: handle low battery. Currently not sure how this is received.

  def eventMap = [
   'On':"active",
   'Off':"inactive",
   'LowBattery':"Low",
   'LightDetected':"Detected"
  ]
  
  def newState = eventMap."${state}"
  
  if (newState == "active" || newState == "inactive")
  {
  	sendEvent (name: "motion", value: "${newState}")
  }
  else if (newState == "Low")
  {
  	sendEvent (name: "battery", value:"${newState}")
  }
  else if (newState == "Detected")
  {
  	sendEvent (name: "luminosity", value:"${newState}")
  }
}
