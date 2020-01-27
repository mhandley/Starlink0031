using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// A min-type priority queue of Nodes
/// </summary>
public class BinaryHeap {

	#region Instance variables
	readonly Node[] data;
    readonly double[] priorities;      
	int count;
    #endregion

    /// <summary>
    /// Creates a new, empty priority queue with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of nodes that will be stored in the queue.</param>
    public BinaryHeap(int capacity) {
		data = new Node[capacity];
		priorities = new double[capacity];
		count = 0;
	}

    /// <summary>
    /// Adds an item to the queue.  Is position is determined by its priority relative to the other items in the queue.
    /// aka HeapInsert
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <param name="priority">Priority value to attach to this item.  Note: this is a min heap, so lower priority values come out first.</param>        
	public void Add(Node item, double priority) {
		if (count == data.Length)
			throw new Exception ("Heap capacity exceeded");

		// Add the item to the heap in the end position of the array (i.e. as a leaf of the tree)
		int position = count++;
		data [position] = item;
		item.QueuePosition = position;
		priorities [position] = priority;
		// Move it upward into position, if necessary
		MoveUp (position);

	}

    /// <summary>
    /// Extracts the item in the queue with the minimal priority value.
    /// </summary>
    /// <returns></returns>
    public Node ExtractMin() 
	{
		Node minNode = data [0];
		Swap (0, count - 1);
		count--;
		MoveDown (0);
		return minNode;
	}

    /// <summary>
    /// Reduces the priority of a node already in the queue.
    /// aka DecreaseKey 
    /// </summary>
    public void DecreasePriority(Node n, double priority) {
		int position = n.QueuePosition;
		while ((position > 0) && (priorities [Parent (position)] > priority)) {
			int original_parent_pos = Parent (position);
			Swap (original_parent_pos, position);
			position = original_parent_pos;
		}
		priorities [position] = priority;
	}

    /// <summary>
    /// Moves the node at the specified position upward, it it violates the Heap Property.
    /// This is the while loop from the HeapInsert procedure in the slides.
    /// </summary>
    /// <param name="position"></param>
    void MoveUp(int position) {
		while ((position > 0) && (priorities [Parent (position)] > priorities [position])) {
			int original_parent_pos = Parent (position);
			Swap (position, original_parent_pos);
			position = original_parent_pos;
		}
	}

        /// <summary>
        /// Moves the node at the specified position down, if it violates the Heap Property
        /// aka Heapify
        /// </summary>
        /// <param name="position"></param>
    void MoveDown(int position) {
		int lchild = LeftChild (position);
		int rchild = RightChild (position);
		int largest = 0;
		if ((lchild < count) && (priorities [lchild] < priorities [position])) {
			largest = lchild;
		} else {
			largest = position;
		}
		if ((rchild < count) && (priorities [rchild] < priorities [largest])) {
			largest = rchild;
		}
		if (largest != position) {
			Swap (position, largest);
			MoveDown (largest);
		}
	}

    /// <summary>
    /// Number of items waiting in queue
    /// </summary>
    public int Count {
		get {
			return count;
		}
	}

    #region Utilities
    /// <summary>
    /// Swaps the nodes at the respective positions in the heap
    /// Updates the nodes' QueuePosition properties accordingly.
    /// </summary>
    void Swap(int position1, int position2) {
		Node temp = data [position1];
		data [position1] = data [position2];
		data [position2] = temp;
		data [position1].QueuePosition = position1;
		data [position2].QueuePosition = position2;

		double temp2 = priorities [position1];
		priorities [position1] = priorities [position2];
		priorities [position2] = temp2;
	}

        /// <summary>
        /// Gives the position of a node's parent, the node's position in the queue.
        /// </summary>
    static int Parent(int position) {
		return (position - 1) / 2;
	}

        /// <summary>
        /// Returns the position of a node's left child, given the node's position.
        /// </summary>
    static int LeftChild(int position) {
		return 2 * position + 1;
	}

        /// <summary>
        /// Returns the position of a node's right child, given the node's position.
        /// </summary>
    static int RightChild(int position) {
		return 2 * position + 2;
	}

    /// <summary>
    /// Checks all entries in the heap to see if they satisfy the heap property.
    /// </summary>
    public void TestHeapValidity() {
		for (int i = 1; i < count; i++)
			if (priorities [Parent (i)] > priorities [i])
				throw new Exception ("Heap violates the Heap Property at position " + i);
	}
    #endregion
}

