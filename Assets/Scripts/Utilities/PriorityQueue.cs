using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Utilities
{
    public class PriorityQueueNode
    {
        public float Priority { get; protected internal set; }
        public int QueueIndex { get; internal set; }
    }
    
    public sealed class PriorityQueue<T> : IEnumerable<T> where T : PriorityQueueNode
    {
        private int m_numNodes;
        private T[] m_nodes;
        
        public PriorityQueue(int maxNodes)
        {
            m_numNodes = 0;
            m_nodes = new T[maxNodes + 1];
        }
        
        public int Count => m_numNodes;

        public int MaxSize => m_nodes.Length - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(m_nodes, 1, m_numNodes);
            m_numNodes = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T node) => m_nodes[node.QueueIndex] == node;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T node, float priority)
        {
            node.Priority = priority;
            m_numNodes++;
            m_nodes[m_numNodes] = node;
            node.QueueIndex = m_numNodes;
            CascadeUp(node);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CascadeUp(T node)
        {
            int parent;
            if(node.QueueIndex > 1)
            {
                parent = node.QueueIndex >> 1;
                T parentNode = m_nodes[parent];
                if(HasHigherOrEqualPriority(parentNode, node)) return;

                //Node has lower priority value, so move parent down the heap to make room
                m_nodes[node.QueueIndex] = parentNode;
                parentNode.QueueIndex = node.QueueIndex;

                node.QueueIndex = parent;
            }
            else
            {
                return;
            }
            
            while(parent > 1)
            {
                parent >>= 1;
                T parentNode = m_nodes[parent];
                if(HasHigherOrEqualPriority(parentNode, node)) break;

                //Node has lower priority value, so move parent down the heap to make room
                m_nodes[node.QueueIndex] = parentNode;
                parentNode.QueueIndex = node.QueueIndex;

                node.QueueIndex = parent;
            }
            
            m_nodes[node.QueueIndex] = node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CascadeDown(T node)
        {
            int finalQueueIndex = node.QueueIndex;
            int childLeftIndex = 2 * finalQueueIndex;

            // If leaf node, we're done
            if(childLeftIndex > m_numNodes) return;

            // Check if the left-child is higher-priority than the current node
            int childRightIndex = childLeftIndex + 1;
            T childLeft = m_nodes[childLeftIndex];
            if(HasHigherPriority(childLeft, node))
            {
                // Check if there is a right child. If not, swap and finish.
                if(childRightIndex > m_numNodes)
                {
                    node.QueueIndex = childLeftIndex;
                    childLeft.QueueIndex = finalQueueIndex;
                    m_nodes[finalQueueIndex] = childLeft;
                    m_nodes[childLeftIndex] = node;
                    return;
                }
                // Check if the left-child is higher-priority than the right-child
                T childRight = m_nodes[childRightIndex];
                if(HasHigherPriority(childLeft, childRight))
                {
                    // left is highest, move it up and continue
                    childLeft.QueueIndex = finalQueueIndex;
                    m_nodes[finalQueueIndex] = childLeft;
                    finalQueueIndex = childLeftIndex;
                }
                else
                {
                    // right is even higher, move it up and continue
                    childRight.QueueIndex = finalQueueIndex;
                    m_nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
            }
            // Not swapping with left-child, does right-child exist?
            else if(childRightIndex > m_numNodes)
            {
                return;
            }
            else
            {
                // Check if the right-child is higher-priority than the current node
                T childRight = m_nodes[childRightIndex];
                if(HasHigherPriority(childRight, node))
                {
                    childRight.QueueIndex = finalQueueIndex;
                    m_nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
                // Neither child is higher-priority than current, so finish and stop.
                else
                {
                    return;
                }
            }

            while(true)
            {
                childLeftIndex = 2 * finalQueueIndex;

                // If leaf node, we're done
                if(childLeftIndex > m_numNodes)
                {
                    node.QueueIndex = finalQueueIndex;
                    m_nodes[finalQueueIndex] = node;
                    break;
                }

                // Check if the left-child is higher-priority than the current node
                childRightIndex = childLeftIndex + 1;
                childLeft = m_nodes[childLeftIndex];
                if(HasHigherPriority(childLeft, node))
                {
                    // Check if there is a right child. If not, swap and finish.
                    if(childRightIndex > m_numNodes)
                    {
                        node.QueueIndex = childLeftIndex;
                        childLeft.QueueIndex = finalQueueIndex;
                        m_nodes[finalQueueIndex] = childLeft;
                        m_nodes[childLeftIndex] = node;
                        break;
                    }
                    // Check if the left-child is higher-priority than the right-child
                    T childRight = m_nodes[childRightIndex];
                    if(HasHigherPriority(childLeft, childRight))
                    {
                        // left is highest, move it up and continue
                        childLeft.QueueIndex = finalQueueIndex;
                        m_nodes[finalQueueIndex] = childLeft;
                        finalQueueIndex = childLeftIndex;
                    }
                    else
                    {
                        // right is even higher, move it up and continue
                        childRight.QueueIndex = finalQueueIndex;
                        m_nodes[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }
                }
                // Not swapping with left-child, does right-child exist?
                else if(childRightIndex > m_numNodes)
                {
                    node.QueueIndex = finalQueueIndex;
                    m_nodes[finalQueueIndex] = node;
                    break;
                }
                else
                {
                    // Check if the right-child is higher-priority than the current node
                    T childRight = m_nodes[childRightIndex];
                    if(HasHigherPriority(childRight, node))
                    {
                        childRight.QueueIndex = finalQueueIndex;
                        m_nodes[finalQueueIndex] = childRight;
                        finalQueueIndex = childRightIndex;
                    }
                    // Neither child is higher-priority than current, so finish and stop.
                    else
                    {
                        node.QueueIndex = finalQueueIndex;
                        m_nodes[finalQueueIndex] = node;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if 'higher' has higher priority than 'lower', false otherwise.
        /// Note that calling HasHigherPriority(node, node) (ie. both arguments the same node) will return false
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasHigherPriority(T higher, T lower) => higher.Priority < lower.Priority;

        /// <summary>
        /// Returns true if 'higher' has higher priority than 'lower', false otherwise.
        /// Note that calling HasHigherOrEqualPriority(node, node) (ie. both arguments the same node) will return true
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasHigherOrEqualPriority(T higher, T lower) => higher.Priority <= lower.Priority;

        /// <summary>
        /// Removes the head of the queue and returns it.
        /// If queue is empty, result is undefined
        /// O(log n)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
            T returnMe = m_nodes[1];
            //If the node is already the last node, we can remove it immediately
            if(m_numNodes == 1)
            {
                m_nodes[1] = null;
                m_numNodes = 0;
                return returnMe;
            }

            //Swap the node with the last node
            T formerLastNode = m_nodes[m_numNodes];
            m_nodes[1] = formerLastNode;
            formerLastNode.QueueIndex = 1;
            m_nodes[m_numNodes] = null;
            m_numNodes--;

            //Now bubble formerLastNode (which is no longer the last node) down
            CascadeDown(formerLastNode);
            return returnMe;
        }

        /// <summary>
        /// Resize the queue so it can accept more nodes.  All currently enqueued nodes are remain.
        /// Attempting to decrease the queue size to a size too small to hold the existing nodes results in undefined behavior
        /// O(n)
        /// </summary>
        public void Resize(int maxNodes)
        {
            var newArray = new T[maxNodes + 1];
            var highestIndexToCopy = Math.Min(maxNodes, m_numNodes);
            Array.Copy(m_nodes, newArray, highestIndexToCopy + 1);
            m_nodes = newArray;
        }

        /// <summary>
        /// Returns the head of the queue, without removing it (use Dequeue() for that).
        /// If the queue is empty, behavior is undefined.
        /// O(1)
        /// </summary>
        public T First => m_nodes[1];

        /// <summary>
        /// This method must be called on a node every time its priority changes while it is in the queue.  
        /// <b>Forgetting to call this method will result in a corrupted queue!</b>
        /// Calling this method on a node not in the queue results in undefined behavior
        /// O(log n)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdatePriority(T node, float priority)
        {
            node.Priority = priority;
            OnNodeUpdated(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnNodeUpdated(T node)
        {
            //Bubble the updated node up or down as appropriate
            var parentIndex = node.QueueIndex >> 1;

            if(parentIndex > 0 && HasHigherPriority(node, m_nodes[parentIndex]))
            {
                CascadeUp(node);
            }
            else
            {
                //Note that CascadeDown will be called if parentNode == node (that is, node is the root)
                CascadeDown(node);
            }
        }

        /// <summary>
        /// Removes a node from the queue.  The node does not need to be the head of the queue.  
        /// If the node is not in the queue, the result is undefined.  If unsure, check Contains() first
        /// O(log n)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(T node)
        {
            //If the node is already the last node, we can remove it immediately
            if(node.QueueIndex == m_numNodes)
            {
                m_nodes[m_numNodes] = null;
                m_numNodes--;
                return;
            }

            //Swap the node with the last node
            var formerLastNode = m_nodes[m_numNodes];
            m_nodes[node.QueueIndex] = formerLastNode;
            formerLastNode.QueueIndex = node.QueueIndex;
            m_nodes[m_numNodes] = null;
            m_numNodes--;

            //Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
            OnNodeUpdated(formerLastNode);
        }

        /// <summary>
        /// By default, nodes that have been previously added to one queue cannot be added to another queue.
        /// If you need to do this, please call originalQueue.ResetNode(node) before attempting to add it in the new queue
        /// If the node is currently in the queue or belongs to another queue, the result is undefined
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetNode(T node) => node.QueueIndex = 0;

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerable<T> e = new ArraySegment<T>(m_nodes, 1, m_numNodes);
            return e.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// <b>Should not be called in production code.</b>
        /// Checks to make sure the queue is still in a valid state.  Used for testing/debugging the queue.
        /// </summary>
        public bool IsValidQueue()
        {
            for(int i = 1; i < m_nodes.Length; i++)
            {
                if(m_nodes[i] != null)
                {
                    int childLeftIndex = 2 * i;
                    if(childLeftIndex < m_nodes.Length && m_nodes[childLeftIndex] != null && HasHigherPriority(m_nodes[childLeftIndex], m_nodes[i]))
                        return false;

                    int childRightIndex = childLeftIndex + 1;
                    if(childRightIndex < m_nodes.Length && m_nodes[childRightIndex] != null && HasHigherPriority(m_nodes[childRightIndex], m_nodes[i]))
                        return false;
                }
            }
            return true;
        }
    }
}