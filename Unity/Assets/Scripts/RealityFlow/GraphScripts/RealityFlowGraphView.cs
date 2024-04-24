using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Linq;
using RealityFlow.Plugin.Scripts;
using Packages.realityflow_package.Runtime.scripts;
using NodeGraphProcessor.Examples;

// This class is the core functionality of whiteboards
// Everything related to whiteboards refers to and utilizes this class
public class RealityFlowGraphView : MonoBehaviour {
	// each whiteboard has a basegraph and an extension of BaseGraph, FlowVSGraph
	public BaseGraph graph;
	public FlowVSGraph vsGraph;

	// used to save the graph
	private JsonElement savePoint; 

	// used to run the graph
	public ProcessGraphProcessor processor;

	public EdgeListener connectorListener;

	// Panels that are on the whiteboard prefab that are enabled and disabled at specific times
	public GameObject Labeled;
	public GameObject contentPanel;
	public GameObject parameterContent;
	public GameObject parameterCreationCanvas;
	public GameObject SelectComparisonCanvas;
	public GameObject VSGraphDropdownCanvas;

	// These are the views we are using, which extend base functionality (edit mode) into play mode
	public GameObject nodePortView;
	public GameObject nodeView;
	public GameObject paramView;
	public GameObject edgeView;

	// Dictionaries to keep track of values of interest
	public Dictionary<string,NodeView> nodeViewDict = new Dictionary<string,NodeView>();
	public static Dictionary<string,RealityFlowGraphView> nodeViewtoRFGVDict = new Dictionary<string,RealityFlowGraphView>();
	public Dictionary<string,NodeView> selectedNVDict = new Dictionary<string,NodeView>();
	public List<BaseNode> selected = new List<BaseNode>();
	public Dictionary<string,ParameterView> paramDict = new Dictionary<string, ParameterView>();

	// Propety to hold value of new node position when node is spawned
	public Vector2 newNodePosition { get; set; } = new Vector2();

	// Coroutine handling variables. The coroutines are not as stable as they should be, for now
	public float updateTimer;
	public float maxUpdateTime;
	public bool reloadCoroutineStarted;
	private bool nodeRoutineRunning = false;
	private bool edgeRoutineRunning = false;
	private bool paramRoutineRunning = false;


	private void Start () {
		updateTimer = 0f;
		maxUpdateTime = 0.05f;
		reloadCoroutineStarted = false;
		SelectComparisonCanvas.SetActive(false);
		parameterCreationCanvas.SetActive(false);
		VSGraphDropdownCanvas.SetActive(false);
		//CreateGraph();
	}

	private void Update(){
		updateTimer += Time.deltaTime;
	}
	
	// Step 1: Figure out which graph to load onto the whiteboard
	public void InitializeGraphStep1()
	{
		VSGraphDropdownCanvas.SetActive(true);
		VSGraphDropdownCanvas.GetComponent<VSGraphSelectionDropdown>().LoadGraphs();
	}

	// Step 2: load it, give it a relevant name, attach a commandpalette, subscribe events, and set a save point
	public void InitializeGraph(FlowVSGraph VSGraph){
		vsGraph = VSGraph;
		graph = (BaseGraph)VSGraph;
		graph.name = (VSGraph.Name + " - " + VSGraph.Id);
		//commandPalette = GameObject.Find("CommandPalette").GetComponent<CommandPalette>();
        graph.onGraphChanges += GraphChangesCallback;
		graph.onExposedParameterListChanged += ParamChangesListChangeCallBack;
		graph.onExposedParameterModified += ParamChangesModifiedCallBack;
		Operations.updateRFGV += ReloadRFGV;
		Operations.runVSGraph += ReceiveRunVSGraph;
		Operations.deleteVSGraph += ReceiveDeleteVSGraph;
		savePoint = JsonSerializer.Serialize(graph);
		// This is a hardload, since loading the graph should delete anything already on the whiteboard
		HardLoadGraph(graph);
	}

	// This method softloads the graph, therefore not deleting anything on the basegraph
	protected void SoftLoadGraph(BaseGraph graph){
		ClearWhiteBoard();
		newNodePosition = new Vector2(-1,-1); // top left of whiteboard, default position

		// Upon load, after whiteboard is cleared, everything needs to be added back onto it
		// this is done via separate coroutines for parameters, nodes, and edges
		// these execute mostly concurrently, which could cause problems
		foreach(ExposedParameter p in graph.exposedParameters){
			StartCoroutine(AddExposedParameterCoroutine(p));
		}
		foreach (BaseNode node in graph.nodes ){
			StartCoroutine (AddNodeCoroutine(node));
		}
		foreach (SerializableEdge edge in graph.edges){
			StartCoroutine( AddEdgeCoroutine(edge));
		}
		// This also needs to checkin the nodeviews
		foreach(KeyValuePair<string,NodeView> nv in nodeViewDict){
			if (nv.Value.CanBeModified){
				nv.Value.CheckIn();
			}
		}
	}

	// Unlike softloadgraph, this method also affects the basegraph
	protected void HardLoadGraph(BaseGraph graph){
		ClearGraph();
		// newNodePosition = new Vector2(-1,-1);
		foreach(ExposedParameter p in graph.exposedParameters){
			StartCoroutine(AddExposedParameterCoroutine(p));
		}
		foreach (BaseNode node in graph.nodes ){
			StartCoroutine (AddNodeCoroutine(node));
		}
		foreach (SerializableEdge edge in graph.edges){
			StartCoroutine( AddEdgeCoroutine(edge));
		}
	}
	
	// When a change is made to the graph, this event will trigger
	void ReloadRFGV()
	{
		updateTimer = 0f;
		if (!reloadCoroutineStarted)
		{
			StartCoroutine (CallSoftReloadCoroutine());
			reloadCoroutineStarted = true;
		}
	}

	// When processGraph is called by another user in the project, this event will trigger
	void ReceiveRunVSGraph(string receivedVSGraphId)
	{
		if (vsGraph.Id == receivedVSGraphId)
		{
			processor = new ProcessGraphProcessor (graph);
			processor.Run ();
		}
	}

	// When a graph is deleted, this event will trigger to fix the whiteboard
	void ReceiveDeleteVSGraph(string receivedVSGraphId)
	{
		if (vsGraph.Id == receivedVSGraphId)
		{
			ClearGraph();
			vsGraph = null;
			graph = null;
		}
	}

	// when we receive a graph update, we don't immediately want to trigger a reload. We want to first check if we get any additional updates in a time
	// frame, and if we do not, then call the reload
	public IEnumerator CallSoftReloadCoroutine() {
		yield return new WaitUntil(() => updateTimer > maxUpdateTime && !nodeRoutineRunning && !edgeRoutineRunning && !paramRoutineRunning);
		SoftLoadGraph(graph);
		updateTimer = 0f;
		reloadCoroutineStarted = false;
	}
	
	// unsubscribe from all events when play mode is exited
	void OnApplicationQuit() 
	{
		// Operations.updateRFGV -= ReloadRFGV;
		// Operations.runVSGraph -= ReceiveRunVSGraph;
		// graph.onExposedParameterListChanged -= ParamChangesListChangeCallBack;
		// graph.onExposedParameterModified -= ParamChangesModifiedCallBack;
		// graph.onGraphChanges -= GraphChangesCallback;
		// Operations.deleteVSGraph -= ReceiveDeleteVSGraph;
	}

	// triggers when the parameter list is updated
	void ParamChangesListChangeCallBack()
	{
		vsGraph.IsUpdated = true;
	}

	// triggers when a parameter value is modified
	void ParamChangesModifiedCallBack(ExposedParameter s)
	{
		vsGraph.IsUpdated = true;
	}

	// these trigger when specific events happen on the basegraph
	// they then set the update flag to true in VSGraph so they can happen on the whiteboard
    void GraphChangesCallback(GraphChanges changes)
    {
        if(changes.addedNode != null)
        {
			vsGraph.IsUpdated = true;
        }
        if(changes.removedNode != null)
        {
			vsGraph.IsUpdated = true;
        }
		if (changes.nodeChanged != null)
		{
			vsGraph.IsUpdated = true;
		}
		if(changes.addedEdge != null)
		{
			vsGraph.IsUpdated = true;
		}
		if(changes.removedEdge != null)
		{
			vsGraph.IsUpdated = true;
		}
    }

	// TODO: implement user-based undo and set up undo to be able to undo more than the last thing
	// This method currently undoes the last thing done in the project, but it is very unstable
	public void UndoLastCommand(){
		// Command cmd;
		// cmd = commandPalette.GetCommandStack()[0];
		// JsonUtility.FromJsonOverwrite(cmd.GetGraphState(), graph);
		// graph.Deserialize();
		// vsGraph.IsUpdated = true;
		// HardLoadGraph(graph);
	}

	// this will delete a selection of nodes, as defined by NodeView.Select()
	public void DeleteSelection(){
		string tmp;
		tmp = JsonUtility.ToJson(graph);

		//commandPalette.AddCommandToStack(new DeleteNodeCommand("Delete Selection of Nodes", tmp));

		foreach(KeyValuePair<string,NodeView> nv in selectedNVDict){
			nodeViewDict.Remove(nv.Key);
			graph.RemoveNode(nv.Value.node);
			nv.Value.Delete();
		}
		selectedNVDict.Clear();
	}

	// Overload of previous method for single node deletion
	public void DeleteSelection(NodeView nv){
		string tmp;
		tmp = JsonUtility.ToJson(graph);

		//commandPalette.AddCommandToStack(new DeleteNodeCommand("Delete Selection of Nodes", tmp));

		nodeViewDict.Remove(nv.node.GUID);
		graph.RemoveNode(nv.node);

	}

	// This method determines the type of node being added to the whiteboard
	// and then launches the AddNodeCoroutine with the proper parameters
	public void AddNodeCommand(string nodeTag, string ObjectName){
		//commandPalette.AddCommandToStack(new AddNodeCommand("Add Node", tmp));

		if(graph == null) {
			InitializeGraph(new FlowVSGraph("TestGraph"));
		}

		// The type of node is determined by the tag on the corresponding nodebrush version
		switch(nodeTag)
		{
			case "TextNode":
				TextNode tn = BaseNode.CreateFromType<TextNode> (newNodePosition);
				graph.AddNode (tn);
				tn.output = "Hello World";
				StartCoroutine (AddNodeCoroutine(tn));
				break;
			case "ColorNode":
				ColorNode con = BaseNode.CreateFromType<ColorNode> (newNodePosition);
				graph.AddNode (con);
				StartCoroutine (AddNodeCoroutine(con));
				break;
			case "ModalNode":
				ModalNode mn = BaseNode.CreateFromType<ModalNode> (newNodePosition);
				graph.AddNode (mn);
				StartCoroutine (AddNodeCoroutine(mn));
				break;
			case "PrintNode":
				PrintNode pn = BaseNode.CreateFromType<PrintNode> (newNodePosition);
				graph.AddNode (pn);
				StartCoroutine (AddNodeCoroutine(pn));
				break;
			case "GameObjectNode":
				GameObjectNode gn = BaseNode.CreateFromType<GameObjectNode> (newNodePosition);
				graph.AddNode (gn);
				GameObject obj = GameObject.Find(ObjectName);
				gn.InitializeNodeFromObject(obj);
				StartCoroutine (AddNodeCoroutine(gn));
				break;
			case "FloatNode":
				FloatNode fn = BaseNode.CreateFromType<FloatNode> (newNodePosition);
				graph.AddNode (fn);
				fn.output = 0.5f;
				StartCoroutine (AddNodeCoroutine(fn));
				break;
			case "IntNode":
				IntNode intn = BaseNode.CreateFromType<IntNode> (newNodePosition);
				graph.AddNode (intn);
				intn.output = 1;
				StartCoroutine (AddNodeCoroutine(intn));
				break;
			case "BoolNode":
				// This case requires input, so it is split into 2 steps
				SelectComparisonCanvas.SetActive(true);
				break;
			case "ConditionalNode":
				IfNode cn = BaseNode.CreateFromType<IfNode> (newNodePosition);
				graph.AddNode(cn);
				StartCoroutine (AddNodeCoroutine(cn));
				break;
			case "StartNode":
				StartNode sn = BaseNode.CreateFromType<StartNode>(newNodePosition);
				graph.AddNode(sn);
				StartCoroutine(AddNodeCoroutine(sn));
				break;
			case "GameObjectManipulationNode":
				GameObjectManipulationNode gmn = BaseNode.CreateFromType<GameObjectManipulationNode>(newNodePosition);
				graph.AddNode(gmn);
				StartCoroutine(AddNodeCoroutine(gmn));
				break;
			default:
				Debug.Log("This case of addnode did not use a tag");
				break; 
		}		
	}

	// This method adds an exposedParameter to a graph as a node
	public void AddParameterNodeToGraph(string epnGUID){
		// string tmp = JsonUtility.ToJson(graph);
		//commandPalette.AddCommandToStack(new AddNodeCommand("Add Parameter to Graph", tmp));

		ParameterNode pn = BaseNode.CreateFromType<ParameterNode> (new Vector2 ());
		pn.parameterGUID = epnGUID;

		graph.AddNode(pn);
		pn.position = new Rect(new Vector2(),new Vector2(100,100));
		StartCoroutine(AddNodeCoroutine(pn));
	}

	// Step 2 of defining a boolNode; where the coroutine is called from
	public void BoolNodeStep2(string comparisonFunction)
	{
		BoolNode bn = BaseNode.CreateFromType<BoolNode> (new Vector2 ());
		bn.compareFunction = comparisonFunction;
		bn.inA = 0f;
		bn.inB = 0f;
		graph.AddNode(bn);
		bn.position = new Rect(new Vector2(newNodePosition.x,newNodePosition.y),new Vector2(100,100));
		StartCoroutine (AddNodeCoroutine(bn));
	}

	// Debug function to print the stack of commands. Useful to visualize undo
	public void PrintCommandStack(){
		//commandPalette.PrintStack();
	}

	// This method is called by Select() in NodeView and simply adds it to the dictionary
	public void AddToSelectionNV(NodeView n){
            selectedNVDict.Add (n.node.GUID,n);
	}

	// Gets user input to define the parameters of AddParameterStep2
	public void AddParameter(){
		parameterCreationCanvas.SetActive(true);
	}

	// How an exposedParameter is created
	public void AddParameterStep2(string parameterType, string parameterName)
	{
		// string tmp = JsonUtility.ToJson(graph);

		// send this to the command palette
		//commandPalette.AddCommandToStack(new AddExposedParameterCommand("Add Exposed Parameter", tmp));

		// The string from the dropdown determines the parameter type
		Type type;
		switch(parameterType)
		{
			default:
			case "GameObject": type = typeof(GameObject); break;
			case "String": type = typeof(string); break;
			case "Float": type = typeof(float); break;
			case "Int": type = typeof(int); break;
			case "Bool": type = typeof(bool); break; 
			case "Color":type = typeof(Color); break;
		}
		graph.AddExposedParameter (parameterName, type, null);
		ExposedParameter epn = graph.GetExposedParameter (parameterName);
		// After the parameter is added to the basegraph, a coroutine is launched
		// to add it to the whiteboard
		StartCoroutine(AddExposedParameterCoroutine(epn));
	}

	public IEnumerator AddExposedParameterCoroutine (ExposedParameter epn){
		// prevents coroutine from running if it is already running
		if(!paramRoutineRunning)
			paramRoutineRunning = true;
		ParameterView newParamView = Instantiate(paramView,new Vector3(),Quaternion.identity).GetComponent<ParameterView> ();
		newParamView.gameObject.transform.SetParent (parameterContent.transform, false);
		newParamView.title.text = epn.name;
		newParamView.type.text = epn.type;
        newParamView.guid.text = epn.guid.Substring (epn.guid.Length - 5);
		newParamView.rfgv = this;
		newParamView.pn = epn;
		paramDict.Add (epn.guid,newParamView);
		// if it is not a gameobject, it must immediately be assigned a value
		// This fixes serialization errors that may occur
		if(newParamView.pn.serializedValue.value == null && newParamView.pn.type != "UnityEngine.GameObject, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")
		{
			newParamView.ModifyParameterValue();
		}
		if(paramDict.Count == graph.exposedParameters.Count)
			paramRoutineRunning = false;
		yield return new WaitForSeconds (.01f);
	}

	// Function to add a parameter modification to the command palette,
	// called by ModifyParameterValue in ParameterView
	public void ModifyExposedParameterValue()
	{
		string tmp = JsonUtility.ToJson(graph);
        //commandPalette.AddCommandToStack(new ModifyExposedParameterCommand("Modify Exposed Parameter", tmp));
	}

	public void RemoveParameter(ParameterView pv)
	{
		string tmp = JsonUtility.ToJson(graph);
        //commandPalette.AddCommandToStack(new DeleteExposedParameterCommand("Delete Exposed Parameter", tmp));
		paramDict.Remove(pv.pn.guid);
		vsGraph.paramIdToObjId.Remove(pv.pn.guid);
		graph.RemoveExposedParameter(pv.pn);
	}

	// A function created by Dr. Murray to create a template graph
	public void CreateGraph () {
		if (graph == null)
		{
			InitializeGraph(new FlowVSGraph("TestGraph"));
		}

		string tmp = JsonUtility.ToJson(graph);

		// send this to the command palette
		//commandPalette.AddCommandToStack(new CreateGraphCommand("Create Graph", tmp));

		graph.AddExposedParameter ("LabelContainer", typeof (GameObject), Labeled);
		TextNode tn = BaseNode.CreateFromType<TextNode> (new Vector2 ());
		graph.AddNode (tn);
		tn.output = "Hello World";
		StartCoroutine (AddNodeCoroutine(tn));
		SetLabelNode sln = BaseNode.CreateFromType<SetLabelNode> (new Vector2 ());
		graph.AddNode (sln);
		graph.Connect (sln.GetPort ("newLabel", null), tn.GetPort ("output", null));
		StartCoroutine (AddNodeCoroutine(sln));
		ParameterNode pn = BaseNode.CreateFromType<ParameterNode> (new Vector2 ());

		pn.parameterGUID = graph.GetExposedParameter ("LabelContainer").guid;
		graph.AddNode (pn);
		graph.Connect (sln.GetPort ("input", ""), pn.GetPort ("output", "output"));
		StartCoroutine (AddNodeCoroutine(pn));
		graph.UpdateComputeOrder ();
		processor = new ProcessGraphProcessor (graph);
    }
	
	public void ConnectEdges(NodePortView input, NodePortView output){
		// string tmp = JsonUtility.ToJson(graph);
        //commandPalette.AddCommandToStack(new AddEdgeCommand("Created Edge", tmp));
		SerializableEdge edge = graph.Connect(input.port, output.port, true);
		StartCoroutine(AddEdgeCoroutine(edge));
		/* Backend:
		 - Create an Edge
		 - Store this edge into the list

		 Frontend:
		 - Create an EdgeView with the edge
		  */
	}
	
	// deletes everything on the graph and whiteboard. Hard clear
	public void ClearGraph () {
		string tmp = JsonUtility.ToJson(graph);
        // commandPalette.AddCommandToStack(new DeleteExposedParameterCommand("Clearing Graph", tmp));

		foreach(KeyValuePair<string,ParameterView> pv in paramDict){
			pv.Value.Delete();
		}
		paramDict.Clear();

		foreach(KeyValuePair<string,NodeView> nv in nodeViewDict){
			nv.Value.Delete();
		}
		nodeViewDict.Clear();
    }

	// clears everything on the whiteboard but does not delete anything from the graph
	public void ClearWhiteBoard()
	{
		foreach(KeyValuePair<string,ParameterView> pv in paramDict){
			pv.Value.DeleteFromWhiteBoard();
		}
		paramDict.Clear();

		foreach(KeyValuePair<string,NodeView> nv in nodeViewDict){
			nv.Value.DeleteFromWhiteBoard();
		}
		nodeViewDict.Clear();
	}

	// Debug function to count the amount of nodes in the graph
    public List <BaseNode> GetNodes()
    {
        Debug.Log("There are " + graph.nodes.Count + " inside runtimegraph");
        return graph.nodes;
    }

	// runs the graph
	public void DoProcessing () {
        // Operations.RunVSGraph(vsGraph.Id, ConfigurationSingleton.SingleInstance.CurrentProject.Id, (_, e) => {/* Debug.Log(e.message);*/ });
        Debug.Log("Doing processing");
        processor = new ProcessGraphProcessor (graph);
		processor.Run ();
	}

	// This coroutine is in charge of asynchronously making the EdgeView Prefab
	public IEnumerator AddEdgeCoroutine (SerializableEdge edge){
		if(!edgeRoutineRunning)
			edgeRoutineRunning = true;
		yield return new WaitUntil(() => !nodeRoutineRunning);
		yield return new WaitForSeconds (.01f);
		EdgeView newEdge = Instantiate( edgeView, new Vector3(), Quaternion.identity).GetComponent<EdgeView>();
		newEdge.gameObject.transform.SetParent (contentPanel.transform, false);
		newEdge.rfgv = this;
		NodeView inputView = nodeViewDict[edge.inputNodeGUID];
		NodeView outputView = nodeViewDict[edge.outputNodeGUID];

		foreach (NodePortView npv in inputView.inputPortViews){
			if (npv.port == edge.inputPort){ newEdge.input = npv; break; }
		}
		foreach (NodePortView npv in outputView.outputPortViews){
			if (npv.port == edge.outputPort){ newEdge.output = npv; break; }
		}
		newEdge.input.edges.Add(newEdge);
		newEdge.output.edges.Add(newEdge);
		// Set the positions of the linerenderer
		newEdge.edge = edge;
		newEdge.Init();

		edgeRoutineRunning = false;
		yield return new WaitForSeconds (.01f);
	}

	// This coroutine adds a NodeView to the whiteboard with a corresponding node
	public IEnumerator AddNodeCoroutine (BaseNode node) {
		if(!nodeRoutineRunning)
			nodeRoutineRunning = true;

		// Instantiate the new node at the position from the ray
        NodeView newView = Instantiate (nodeView, new Vector3(node.position.x, node.position.y, 0f), 
			Quaternion.identity).GetComponent<NodeView>();

		// Set the nodes parent in the scene heirarchy
        newView.gameObject.transform.SetParent (contentPanel.transform, false);

		// Set the rectTransform position here after we've set the parent
        newView.title.text = node.name;
        newView.node = node;
		// check if the node is a parameter. If it is, newView.GUID.text = node.name;
        newView.GUID.text = node.GUID.Substring (node.GUID.Length - 5);
		newView.rfgv = this;
		newView.nodeGUID = node.GUID;
        foreach (NodePort input in node.inputPorts) {
            NodePortView npv = Instantiate (nodePortView).GetComponent<NodePortView> ();
            npv.gameObject.transform.SetParent (newView.inputPanel.transform, false);
            npv.gameObject.GetComponent<RectTransform> ().SetAsLastSibling ();
			npv.listener = connectorListener;
			npv.type = "input";
            yield return new WaitForSeconds (.01f);
            npv.Init (input);
			newView.inputPortViews.Add(npv);
        }
        foreach (NodePort output in node.outputPorts) {
            NodePortView npv = Instantiate (nodePortView).GetComponent<NodePortView> ();
            npv.gameObject.transform.SetParent (newView.outputPanel.transform,false);
            npv.gameObject.GetComponent<RectTransform> ().SetAsLastSibling ();
			npv.listener = connectorListener;
			npv.type = "output";
            yield return new WaitForSeconds (.01f);
            npv.Init (output);
			newView.outputPortViews.Add(npv);
        }
        nodeViewDict.Add (node.GUID,newView);

		if(nodeViewtoRFGVDict.ContainsKey(node.GUID))
		{
			nodeViewtoRFGVDict[node.GUID] = this;
		}
		else
		{
			nodeViewtoRFGVDict.Add(node.GUID,this);
		}

		// Operations.CheckinNodeView(newView.nodeGUID, ConfigurationSingleton.SingleInstance.CurrentProject.Id, (_, e) => {});

		RectTransform rect = newView.gameObject.GetComponent<RectTransform> ();
        rect.SetAsLastSibling ();

		// This check determines if the coroutine should be done running
		// by seeing if the amount of nodeviews matches the amount of nodes
		if(nodeViewDict.Count == graph.nodes.Count)
			nodeRoutineRunning = false;
        yield return new WaitForSeconds (.01f);
    }
}