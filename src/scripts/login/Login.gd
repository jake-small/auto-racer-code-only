extends Control

func _ready():
	Firebase.Auth.connect("signup_succeeded", self, "_on_FirebaseAuth_signup_success")
	Firebase.Auth.connect("login_succeeded", self, "_on_FirebaseAuth_login_success")
	Firebase.Auth.connect("login_failed", self, "_on_FirebaseAuth_login_failed")
	Firebase.Auth.connect("auth_request", self, "_on_FirebaseAuth_auth_request")
	print("Logging in anonymously")
	Firebase.Auth.check_auth_file()

func _on_FirebaseAuth_signup_success(auth_info):
	print("Signup successful")
	Firebase.Auth.save_auth(auth_info)
	get_tree().change_scene("res://src/scenes/menus/MainMenu.tscn")
	
func _on_FirebaseAuth_login_success():
	print("Login successful")
	
func _on_FirebaseAuth_login_failed(error_code, message):
	print("Login failed")
	print("error code: " + str(error_code))
	print("message: " + str(message))
	get_node("Label_login_info").text = "error code: " + str(error_code) + " message: " + str(message)

# Emitted for each Auth request issued.
# `result_code` -> Either `1` if auth succeeded or `error_code` if unsuccessful auth request
# `result_content` -> Either `auth_result` if auth succeeded or `error_message` if unsuccessful auth request
func _on_FirebaseAuth_auth_request(result_code, result_content):
	if (result_code == ERR_DOES_NOT_EXIST || result_code == 400):
		get_node("Label_login_info").text = "logging in anonymously..."
		Firebase.Auth.login_anonymous()
	else:
		get_node("Label_login_info").text = "log in successfull"
		get_tree().change_scene("res://src/scenes/menus/MainMenu.tscn")
		
func _on_Login_button_up():
	print("Login button clicked")
	Firebase.Auth.check_auth_file()
