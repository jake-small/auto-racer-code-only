extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	Firebase.Auth.connect("signup_succeeded", self, "_on_FirebaseAuth_signup_success")
	Firebase.Auth.connect("login_succeeded", self, "_on_FirebaseAuth_login_success")
	Firebase.Auth.connect("login_failed", self, "_on_FirebaseAuth_login_failed")

func _on_FirebaseAuth_signup_success(auth_info):
	print("Signup successful")
	print(auth_info)
	get_tree().change_scene("res://src/scenes/menus/MainMenu.tscn")
	
func _on_FirebaseAuth_login_success():
	print("Login successful")
	
func _on_FirebaseAuth_login_failed(error_code, message):
	print("Login failed")
	print("error code: " + str(error_code))
	print("message: " + str(message))
	get_node("Label_login_info").text = "error code: " + str(error_code) + " message: " + str(message)

func _on_Login_button_up():
	print("Login button clicked")
	Firebase.Auth.login_anonymous()
