extends Node2D

var playerId : String

func _ready():
	Firebase.Auth.connect("userdata_received", self, "_on_FirebaseAuth_userdata_received")
	Firebase.Auth.get_user_data()

#func _on_task_query_error(error_code, message):
#	print("task query error " + str(error_code) + " message " + message)
#
#func _on_task_error(error_code, message):
#	print("task error " + str(error_code) + " message " + message)

func _on_FirebaseAuth_userdata_received(userdata: FirebaseUserData):
	playerId = "anon-" + str(userdata.local_id)
	
func SendPlayerTurn(turnGuid: String, playerName: String, skin: String, turn: int, cards: Array, cardVersion: String) -> void:
	print("Sending player turn to firestore")
	var player_turn = {'player_name':playerName, 'player_id':playerId, 'skin':skin, 'turn':turn,
	'amount_used':0, "timestamp":OS.get_datetime(), 'cards':cards, 'card_version':cardVersion}
	var cardMajorVersion = cardVersion.left(cardVersion.find('.'))
	var firestore_collection : FirestoreCollection = Firebase.Firestore.collection("player_turns_v" + cardMajorVersion)
	var add_task : FirestoreTask = firestore_collection.add(turnGuid, player_turn)
	var document : FirestoreTask = yield(add_task, "task_finished")
	print(document)
	
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
