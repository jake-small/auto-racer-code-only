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

func on_document_error(error, errorCode, errorMessage):
	print("firebase document error")
	print(error)
	print(errorCode)
	print(errorMessage)

func _on_FirebaseAuth_userdata_received(userdata: FirebaseUserData):
	playerId = "anon-" + str(userdata.local_id)
	
func SendPlayerTurn(csharpNode: Node, turnGuid: String, playerName: String, skin: String, turn: int, cards: Array, cardVersion: String) -> void:
	print("Sending player turn to firestore")
	var player_turn = {'player_name':playerName, 'player_id':playerId, 'skin':skin, 'turn':turn,
	'amount_used':0, "timestamp":OS.get_datetime(), 'cards':cards, 'card_version':cardVersion}
	var cardMajorVersion = cardVersion.left(cardVersion.find('.'))
	var firestore_collection : FirestoreCollection = Firebase.Firestore.collection("player_turns_v" + cardMajorVersion)
	var add_task : FirestoreTask = firestore_collection.add(turnGuid, player_turn)
	var document : FirestoreTask = yield(add_task, "task_finished")
	GetOpponentTurns(csharpNode, turn, cardMajorVersion)
	
func GetOpponentTurns(csharpNode: Node, turn: int, cardMajorVersion: String):
	print("Getting opposingn player turns from firestore")
	var query : FirestoreQuery = FirestoreQuery.new()
	query.from("player_turns_v" + cardMajorVersion).where("turn", FirestoreQuery.OPERATOR.EQUAL, turn).limit(3)
	var query_task : FirestoreTask = Firebase.Firestore.query(query)
	var result : FirestoreTask = yield(query_task, "task_finished")
	var playerTurnArr = []
	for i in range(0, result.data.size()):
		playerTurnArr.append(result.data[i].doc_fields)
	csharpNode.SetupRace(playerTurnArr)
