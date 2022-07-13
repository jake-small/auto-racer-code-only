extends Node2D

const collectionPrefix = "player_turns_v"
const retryAttempts = 5

var playerId : String
var playerName : String
var rng = RandomNumberGenerator.new()

func _ready():
	Firebase.Auth.connect("userdata_received", self, "_on_FirebaseAuth_userdata_received")
	Firebase.Auth.connect("request_completed", self, "_on_request_completed")
	Firebase.Firestore.connect("task_error", self, "_on_task_error")
#	Firebase.Auth.connect("task_query_error", self, "_on_task_query_error")
#	Firebase.Auth.connect("task_error", self, "_on_task_error")
	Firebase.Auth.get_user_data()
	rng.randomize()
	
#func _on_task_query_error(error_code, message):
#	print("task query error " + str(error_code) + " message " + message)
#	get_node("Label_errors").text = "task query error " + str(error_code) + " message " + message

func _on_task_error(error_code, message):
	print("task error " + str(error_code) + " message " + message)
	get_node("Label_errors").text = "task error " + str(error_code) + " message " + message

func _on_request_completed(result : int, response_code : int, headers : PoolStringArray, body : PoolByteArray) -> void:
	if response_code != 200:
		print("received bad firebase response: " + str(response_code))
		get_node("Label_errors").text = "received bad firebase response: " + str(response_code)
	return
	
#func on_document_error(error, errorCode, errorMessage):
#	print("firebase document error")
#	print(error)
#	print(errorCode)
#	print(errorMessage)
#	get_node("Label_errors").text = "firebase document error: " + str(error) + " code:" + str(errorCode) + " msg: " + str(errorMessage)
	
func _on_FirebaseAuth_userdata_received(userdata: FirebaseUserData):
	playerId = str(userdata.local_id)
	playerName = "anon-" + str(userdata.local_id)
	
func SendPlayerTurn(csharpNode: Node, characterName: String, skin: String, numOpponents: int, turn: int, cards: Array, cardVersion: String) -> void:
	var cardMajorVersion = cardVersion.left(cardVersion.find('.'))
	var opponentTurns = yield(GetOpponentTurns(numOpponents, turn, cardMajorVersion, characterName), "completed")
	if opponentTurns == null:
		opponentTurns = []
	
	print("Sending player turn to firestore")
#	get_node("Label_errors").text = "Sending player turn to firestore"
	var random64bit = str(rng.randi()) + str(rng.randi())
	var player_turn = {'character_name': characterName, 'player_name':playerName, 'player_id':playerId, 'skin':skin, 'turn':turn,
		'amount_used':0, "timestamp":OS.get_datetime(), 'cards':cards, 'card_version':cardVersion, 'random':random64bit}
	var firestore_collection : FirestoreCollection = Firebase.Firestore.collection(collectionPrefix + cardMajorVersion)
	var add_task : FirestoreTask = firestore_collection.add("", player_turn)
	var document : FirestoreTask = yield(add_task, "task_finished")
	if document.error:
		print("Error sending turn to firestore with message: " + document.error["message"])
#		get_node("Label_errors").text = "Error sending turn to firestore with message: " + document.error["message"]
	csharpNode.SetupRace(opponentTurns)
	
func GetOpponentTurns(numOpponents: int, turn: int, cardMajorVersion: String, characterName: String):
	print("Getting opposing player turns from firestore")
#	get_node("Label_errors").text = "Getting opposing player turns from firestore"
	var opponents = []
	if numOpponents < 1:
		yield(get_tree().create_timer(0.01), "timeout")
		return opponents
	var characterNameFilter = [characterName]
	print("Querying for opponent 1")
#	get_node("Label_errors").text = "Querying for opponent 1"
	var opponent1 = yield(QueryPlayerTurn(turn, cardMajorVersion, characterNameFilter), "completed")
	if opponent1 != null:
		characterNameFilter.append(opponent1["character_name"])
		opponents.append(opponent1)
	if numOpponents < 2:
		yield(get_tree().create_timer(0.01), "timeout")
		return opponents
	print("Querying for opponent 2")
#	get_node("Label_errors").text = "Querying for opponent 1, 2"
	var opponent2 = yield(QueryPlayerTurn(turn, cardMajorVersion, characterNameFilter), "completed")
	if opponent2 != null:
		characterNameFilter.append(opponent2["character_name"])
		opponents.append(opponent2)
	if numOpponents < 3:
		yield(get_tree().create_timer(0.01), "timeout")
		return opponents
	print("Querying for opponent 3")
#	get_node("Label_errors").text = "Querying for opponent 1, 2, 3"
	var opponent3 = yield(QueryPlayerTurn(turn, cardMajorVersion, characterNameFilter), "completed")
	if opponent3 != null:
		characterNameFilter.append(opponent3["character_name"])
		opponents.append(opponent3)
	yield(get_tree().create_timer(0.01), "timeout")
	return opponents

func QueryPlayerTurn(turn: int, cardMajorVersion: String, characterNameFilter: Array, lookLeft: bool = true, attemptNum: int = 0) -> Dictionary:
	if attemptNum > retryAttempts:
		print("Unable to find a document after " + str(retryAttempts) + " attempts, stopping...")
		# have to delay, otherwise will crash because it's returning too fast, before the signal can be registered
		# see bottom of this post: https://godotengine.org/qa/56823/first-argument-of-yield-not-of-type-object
		yield(get_tree().create_timer(0.01), "timeout")
		return null
	print("Querying for opponent turn, attempt number " + str(attemptNum))
#	get_node("Label_errors").text = "Querying for opponent turn, attempt number " + str(attemptNum)
	var random64bit = str(rng.randi()) + str(rng.randi())
#	get_node("Label_errors").text = "Querying for opponent turn, attempt number " + str(attemptNum) + ". Generated random number."
	var query = FirestoreQuery.new()
	query.from(collectionPrefix + cardMajorVersion)
	query.where("turn", FirestoreQuery.OPERATOR.EQUAL, turn, FirestoreQuery.OPERATOR.AND)
	if lookLeft:
		query.where("random", FirestoreQuery.OPERATOR.LESS_THAN_OR_EQUAL, random64bit)
		query.order_by("random", FirestoreQuery.DIRECTION.DESCENDING)
	else:
		query.where("random", FirestoreQuery.OPERATOR.GREATER_THAN_OR_EQUAL, random64bit)
		query.order_by("random", FirestoreQuery.DIRECTION.ASCENDING)
	query.limit(1)
#	get_node("Label_errors").text = "Querying for opponent turn, attempt number " + str(attemptNum) + ". Querying..."
	var query_task : FirestoreTask = Firebase.Firestore.query(query)
	# this yield OCCASSIONALLY doesn't on android html5 PWA....
	var result : FirestoreTask = yield(query_task, "task_finished")
#	get_node("Label_errors").text = "Querying for opponent turn, attempt number " + str(attemptNum) + ". Got response"
	if result.error:
		print("Result error message: " + result.error["message"])
		get_node("Label_errors").text = "Result error message: " + result.error["message"]
		yield(get_tree().create_timer(0.01), "timeout")
		return null
	var opponent = {}
	if result.data == null || result.data.empty():
		if lookLeft:
			print("Unable to find a document to the left, trying again")
#			get_node("Label_errors").text = "Unable to find a document to the left, trying again"
			opponent = yield(QueryPlayerTurn(turn, cardMajorVersion, characterNameFilter, false, attemptNum), "completed")
		else:
			print("Unable to find a document to the right, stopping...")
#			get_node("Label_errors").text = "Unable to find a document to the right, stopping..."
			yield(get_tree().create_timer(0.01), "timeout")
			return null
	else:
		opponent = result.data[0].doc_fields
	if opponent == null || opponent == {} || !opponent.has("character_name"):
		print("Unable to find opponent, stopping...")
#		get_node("Label_errors").text = "Unable to find opponent, stopping..."
		yield(get_tree().create_timer(0.01), "timeout")
		return null
	if opponent["character_name"] in characterNameFilter:
		print("Duplicate opponent found, trying again")
#		get_node("Label_errors").text = "Duplicate opponent found, trying again"
		opponent = yield(QueryPlayerTurn(turn, cardMajorVersion, characterNameFilter, true, attemptNum + 1), "completed")
	print("Opponent found!")
#	get_node("Label_errors").text = "Opponent found!"
	return opponent
