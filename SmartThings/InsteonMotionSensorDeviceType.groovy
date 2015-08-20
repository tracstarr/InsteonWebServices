/*
 * Insteon Motion Device
 */

metadata {
  definition (name: "Insteon Motion Sensor", author: "tracstarr <tracstarr@hotmail.com>", namespace:"bitbounce") {
    capability "Motion Sensor"
    capability "Sensor"
    capability "Battery"    
  }

  simulator {
  }

  tiles {
    standardTile("motion", "device.motion", width: 2, height: 2, canChangeBackground: true, canChangeIcon: true) {
      state("active",   label:'motion',    icon:"st.motion.motion.active",   backgroundColor:"#53a7c0")
      state("inactive", label:'no motion', icon:"st.motion.motion.inactive", backgroundColor:"#ffffff")      
    }
    
    valueTile("battery", "device.battery", inactiveLabel: false, decoration: "flat") {
			state "battery", label:'${currentValue}% battery', unit:""
		}

    main "motion"
    details(["motion", "battery"])
  }
}

// Called from SmartApp
def update(String state) {

  //TODO: handle low battery. Currently not sure how this is received.

  def eventMap = [
   'On':"active",
   'Off':"inactive"   
  ]
  
  def newState = eventMap."${state}"
  sendEvent (name: "motion", value: "${newState}")
}
