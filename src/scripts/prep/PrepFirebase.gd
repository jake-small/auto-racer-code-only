extends Node2D

const collectionPrefix = "test_v"#"player_turns_v"
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
	var firestore_collection : FirestoreCollection = Firebase.Firestore.collection(collectionPrefix + cardMajorVersion)
	var add_task : FirestoreTask = firestore_collection.add("", player_turn)
	var document : FirestoreTask = yield(add_task, "task_finished")
	GetOpponentTurns(csharpNode, turn, cardMajorVersion, playerName)
	
func GetOpponentTurns(csharpNode: Node, turn: int, cardMajorVersion: String, playerName: String):
	print("Getting opposing player turns from firestore")
	var opponent1 = yield(QueryPlayerTurn(turn, cardMajorVersion, [playerName]), "completed")
	if opponent1 == null:
		csharpNode.FailedToLoadFirebaseOpponents()
		return
	var opponent2 = yield(QueryPlayerTurn(turn, cardMajorVersion, [playerName, opponent1["player_name"]]), "completed")
	if opponent2 == null:
		csharpNode.FailedToLoadFirebaseOpponents()
		return
	var opponent3 = yield(QueryPlayerTurn(turn, cardMajorVersion, [playerName, opponent1["player_name"], opponent2["player_name"]]), "completed")
	if opponent3 == null:
		csharpNode.FailedToLoadFirebaseOpponents()
		return
	csharpNode.SetupRace([opponent1, opponent2, opponent3])

func QueryPlayerTurn(turn: int, cardMajorVersion: String, playerNameFilter: Array) -> Dictionary:
	var query : FirestoreQuery = FirestoreQuery.new()
	query.from(collectionPrefix + cardMajorVersion)
	query.where("turn", FirestoreQuery.OPERATOR.EQUAL, turn, FirestoreQuery.OPERATOR.AND)
	query.where("player_name", FirestoreQuery.OPERATOR.NOT_IN, playerNameFilter)
	query.limit(1)
	var query_task : FirestoreTask = Firebase.Firestore.query(query)
	var result : FirestoreTask = yield(query_task, "task_finished")
	if result.error:
		print(result.error["message"])
		return null
	if result.data == null || result.data.empty():
		print("error in QueryPlayerTurn(): no documents found")
		return null
	return result.data[0].doc_fields
