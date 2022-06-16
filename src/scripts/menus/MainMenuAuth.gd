extends Label


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	Firebase.Auth.connect("userdata_received", self, "_on_FirebaseAuth_userdata_received")
	Firebase.Auth.get_user_data()


func _on_FirebaseAuth_userdata_received(userdata: FirebaseUserData):
	self.text = "anon-" + str(userdata.local_id)
