using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Engine.Utilities
{
    public enum DecisionOutcome
    {
        SUCCESSFUL,
        INTERRUPTED,
        UNSUCCESSFUL
    };

    public delegate void DecisionOutcomeHandler(DecisionOutcome outcome);

    public class DecisionTreeNode
    {
        public Decision value;
        public List<Pair<Predicate<List<object>>, DecisionTreeNode>> connections;

        public DecisionTreeNode(Decision decision)
        {
            value = decision;
            connections = new List<Pair<Predicate<List<object>>, DecisionTreeNode>>();
        }
    }

    public class DecisionTree
    {
        public DecisionTreeNode root = new DecisionTreeNode(null);
        Decision currentlyProcessedDecision = null;

        /// <summary>
        /// Creates a new node in decision tree and returns it.
        /// Returns null when parent isn't specified.
        /// </summary>
        public DecisionTreeNode AddNode(DecisionTreeNode parent, Predicate<List<object>> edgeCondition, Decision decision = null)
        {
            if (parent == null) return null;
            DecisionTreeNode node = new DecisionTreeNode(decision);
            node.value.EndDecision = EndDecision;
            parent.connections.Add(new Pair<Predicate<List<object>>, DecisionTreeNode>(edgeCondition, node));
            return node;
        }

        public void Update(GameTime gameTime, ref List<object> attributes)
        {
            if (currentlyProcessedDecision != null) currentlyProcessedDecision.Update(gameTime, ref attributes);
            else Decide(root, attributes);
        }

        private void EndDecision(DecisionOutcome outcome)
        {
            // May need to expand this to make it possible to do other stuff depending on outcome.
            currentlyProcessedDecision = null;
        }

        public void Decide(DecisionTreeNode node, List<object> attributes)
        {
            if(node.value != null)
            {
                currentlyProcessedDecision = node.value;
                currentlyProcessedDecision.Initialize(attributes);
                return;
            }

            foreach(Pair<Predicate<List<object>>, DecisionTreeNode> pair in node.connections)
            {
                if(pair.First(attributes))
                {
                    Decide(pair.Second, attributes);
                }
            }
        }

        public DecisionTree()
        { }

        public DecisionTree(Decision rootDecision)
        {
            root = new DecisionTreeNode(rootDecision);
            if (rootDecision != null) root.value.EndDecision = EndDecision;
        }
    }

    public abstract class Decision
    {
        /// <summary>
        /// Handler used to end the decision life span and return control to decision tree with decision outcome.
        /// </summary>
        public DecisionOutcomeHandler EndDecision = null;

        /// <summary>
        /// Override this method to implement logic for decision.
        /// </summary>
        public virtual void Update(GameTime gameTime, ref List<object> attributes) {}

        /// <summary>
        /// Override this method to implement logic for initializing your variables
        /// for when the decision is chosen and starts its logic.
        /// </summary>
        public virtual void Initialize(List<object> attributes) {}
    }
}
