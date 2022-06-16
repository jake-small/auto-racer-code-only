extends Node2D

var uuid : String

func _ready():
	Firebase.Auth.connect("userdata_received", self, "_on_FirebaseAuth_userdata_received")
	Firebase.Auth.get_user_data()

#func _on_task_query_error(error_code, message):
#	print("task query error " + str(error_code) + " message " + message)
#
#func _on_task_error(error_code, message):
#	print("task error " + str(error_code) + " message " + message)

func _on_FirebaseAuth_userdata_received(userdata: FirebaseUserData):
	uuid = "anon-" + str(userdata.local_id)
	
func StartRace(node : Node) -> void:
	SendPlayerTurn()
	
func SendPlayerTurn():
	print("Sending player turn to firestore")
	var firestore_collection : FirestoreCollection = Firebase.Firestore.collection("test_collection_v0_1_0")
#	var add_task : FirestoreTask = firestore_collection.add("my_uuid", {'player_name': 'anon', 'card':'curse', 'slot': 0})
#	var added_player_turn : FirestoreDocument = yield(add_task, "task_finished")
#	var player_turn = {'player_name': 'anon', 'card':'curse', 'slot': 0}
	var date = GetDate()
	var cards = {'uuid': 'asdf', 'base_move':5, 'slot':1}
	var player_turn = {'player_name':'anon the anonymous', 'player_uuid':uuid, 'turn':1, 'amount_used':0, 'date': date, 'cards': cards}
	var add_task : FirestoreTask = firestore_collection.add("new_uuid2", player_turn)
	var document : FirestoreTask = yield(add_task, "task_finished")
	print(document)
	GetOpponentTurns()
	
func GetOpponentTurns():
#	var query : FirestoreQuery = FirestoreQuery.new()
#	query.from("player_turns_v0.1.0")
#	query.where("turn", FirestoreQuery.OPERATOR.EQUAL, 1)
#	query.order_by("date", FirestoreQuery.DIRECTION.DESCENDING)
#	query.limit(3)
#	var result : Array = yield(Firebase.Firestore.query(query), "result_query")
#	var query : FirestoreQuery = FirestoreQuery.new().from("player_turns_v0.1.0").where("turn", FirestoreQuery.OPERATOR.EQUAL, 1).limit(10)
	var query : FirestoreQuery = FirestoreQuery.new().from("test_collection_v0_1_0").limit(3)
	var result = yield(Firebase.Firestore.query(query), "result_query")
	print(result)

func GetDate():
	var time = OS.get_datetime()
#	var nameweekday= ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"]
#	var namemonth= ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
#	var dayofweek = time["weekday"]   # from 0 to 6 --> Sunday to Saturday
	var day = time["day"]                         #   1-31
	var month= time["month"]               #   1-12
	var year= time["year"]             
	var hour= time["hour"]                     #   0-23
	var minute= time["minute"]             #   0-59
	var second= time["second"]             #   0-59
#	var dateRFC1123 = "%s, %02d %s %d %02d:%02d:%02d GMT" % [nameweekday[dayofweek], day, namemonth[month-1], year, hour, minute, second]
	return "%d-%d-%d-%02d:%02d:%02d GMT" % [year, month, day, hour, minute, second]
