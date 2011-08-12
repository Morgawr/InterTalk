using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterTalk
{
    /// <summary>
    /// This class is the singleton interface for the event manager core. Every class that wants to subscribe to a certain event has to register to
    /// this manager, every class firing a condition/event has to talk through this class.
    /// </summary>
    public sealed class InterManagerCore
    {

        #region Singleton Implementation

        private static readonly InterManagerCore instance = new InterManagerCore();

        private InterManagerCore() 
        {
            conditions = new List<Dictionary<string, List<Tuple<Delegate,object[]>>>>();
            safetyBox = new List<Dictionary<string, object>>();
            toRemove = new List<Tuple<int, string>>();
        }

        /// <summary>
        /// Instance of the singleton.
        /// </summary>
        public static InterManagerCore Instance
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region Fields


        /// <summary>
        /// List that contains all the different conditions in different layers (for multi-purpose event handling)
        /// </summary>
        private List<Dictionary<String, List<Tuple<Delegate,object[]>>>> conditions;

        /// <summary>
        /// List that contains a "safety box" for the invoked methods to communicate some important data to the registered methods.
        /// </summary>
        private List<Dictionary<String, object>> safetyBox;

        /// <summary>
        /// List of conditions to remove from the instance after Invoke has finished.
        /// </summary>
        private List<Tuple<int, string>> toRemove;

        /// <summary>
        /// Value to check if the instance is invoking any condition or not.
        /// </summary>
        private bool isInvoking = false;

        /// <summary>
        /// Value to check if the instance is invoking any condition or not.
        /// </summary>
        public bool IsInvoking
        {
            get
            {
                return isInvoking;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// This method registers a new listener to a certain event with certain parameters as a callback function
        /// </summary>
        /// <param name="depth">Depth of the layer, multiple layers with same condition keys can be registered.</param>
        /// <param name="condition">String name for the conditon/event to listen to.</param>
        /// <param name="handler">Callback method (delegate) when the event is fired.</param>
        /// <param name="args">Array of objects for the delegate method parameters.</param>
        /// <returns>Returns an int representing the ID of the registered listener. (Required to unregister a listener).</returns>
        public int Register(int depth, String condition, Delegate handler, params object[] args)
        {
            Tuple<Delegate,object[]> tp = new Tuple<Delegate,object[]>(handler,args);

            while (conditions.Count <= depth)
            {
                conditions.Add(new Dictionary<String, List<Tuple<Delegate,object[]>>>());
                safetyBox.Add(new Dictionary<string, object>());
            }

            if (!conditions[depth].ContainsKey(condition))
            {
                conditions[depth][condition] = new List<Tuple<Delegate,object[]>>();
                safetyBox[depth] = new Dictionary<string, object>();
            }

            int id;
            if((id = ObtainFirstNullID(depth,condition)) != -1)
            {
                conditions[depth][condition][id] = tp;
            }
            else
            {
                conditions[depth][condition].Add(tp);
                id = conditions[depth][condition].LastIndexOf(tp);
            }
            
            return id;
        }


        /// <summary>
        /// Obtains the first null ID in the conditions list.
        /// </summary>
        /// <param name="depth">The layer number for the condition we need.</param>
        /// <param name="condition">The condition we are going to check.</param>
        /// <returns>The ID value.</returns>
        private int ObtainFirstNullID(int depth, string condition)
        {
            for (int i = 0; i < conditions[depth][condition].Count; i++)
            {
                if (conditions[depth][condition][i] == null)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// This method fires the event for all the callbacks registered to a certain event.
        /// </summary>
        /// <param name="depth">Level of depth for the layer for the invoked event.</param>
        /// <param name="condition">Condition for the invoked event.</param>
        /// <param name="message">Extra messages the invoker wants to make the subscribers aware of.</param>
        /// <param name="multiThreaded">A boolean that checks if the manager should optimize the event call using multi-threading.</param>
        /// <returns>False if there were problems, else true.</returns>
        public bool Invoke(int depth, String condition, object message, bool multiThreaded)
        {
            if (depth >= safetyBox.Count || !safetyBox[depth].ContainsKey(condition))
                return false;

            isInvoking = true;

            safetyBox[depth][condition] = message;

            if (multiThreaded)
            {
                Parallel.ForEach(conditions[depth][condition], tp =>
                    {
                        if (tp != null)
                            tp.Item1.DynamicInvoke(tp.Item2);
                    });
            }
            else
            {
                foreach (Tuple<Delegate, object[]> tp in conditions[depth][condition])
                {
                    if (tp != null)
                        tp.Item1.DynamicInvoke(tp.Item2);

                }
            }
            
            safetyBox[depth][condition] = null;
            isInvoking = false;

            foreach (Tuple<int, string> T in toRemove)
                realReset(T.Item1, T.Item2);

            toRemove.Clear();

            return true;
        }

        /// <summary>
        /// This method fires the event for all the callbacks registered to a certain event without using multithreading.
        /// </summary>
        /// <param name="depth">Level of depth for the layer for the invoked event.</param>
        /// <param name="condition">Condition for the invoked event.</param>
        /// <param name="message">Extra messages the invoker wants to make the subscribers aware of.</param>
        /// <returns>False if there were problems, else true.</returns>
        public bool Invoke(int depth, String condition, object message)
        {
            return Invoke(depth, condition, message, false);
        }

        /// <summary>
        /// This method unregisters a listener's callback method from a certain condition.
        /// </summary>
        /// <param name="depth">Level of depth for the layer of the condition.</param>
        /// <param name="condition">Condition the listener wants to stop listening.</param>
        /// <param name="ID">ID of the listener.</param>
        public void Unregister(int depth, String condition, int ID)
        {
            conditions[depth][condition][ID] = null;
        }

        /// <summary>
        /// This method returns the safety box related to a certain condition in a certain layer.
        /// </summary>
        /// <param name="depth">The layer number.</param>
        /// <param name="condition">The string condition of the registered event.</param>
        /// <returns>Returns the message stored in the safety box. Always returns null if this method is called outside a fired event.</returns>
        public object MySafetyBox(int depth, String condition)
        {
            return safetyBox[depth][condition];
        }


        /// <summary>
        /// Gets the number of registered methods to this event.
        /// </summary>
        /// <param name="depth">The depth of the layer.</param>
        /// <param name="condition">The condition of the event.</param>
        /// <returns>The number of items subscribed.</returns>
        public int GetRegistered(int depth, String condition)
        {
            if (depth >= conditions.Count || !conditions[depth].ContainsKey(condition))
                return 0;
            return conditions[depth][condition].Count;
        }

        /// <summary>
        /// Resets the instance.
        /// </summary>
        public void Reset()
        {
            if (IsInvoking)
                toRemove.Add(new Tuple<int, string>(-1, ""));
            else
                realReset(-1, String.Empty);
        }

        /// <summary>
        /// Resets the instance at a certain event removing all the subscribed methods.
        /// </summary>
        /// <param name="depth">The depth of the layer.</param>
        /// <param name="condition">The condition of the event.</param>
        public void Reset(int depth, String condition)
        {
            if (depth >= conditions.Count || !conditions[depth].ContainsKey(condition))
                return;

            if (IsInvoking)
                toRemove.Add(new Tuple<int, string>(depth, condition));
            else
                realReset(depth, condition);
        }

        /// <summary>
        /// Resets the instance at a certain depth level.
        /// </summary>
        /// <param name="depth">The depth of the layer.</param>
        public void Reset(int depth)
        {
            if (depth >= conditions.Count)
                return;
            
            if(IsInvoking)
                toRemove.Add(new Tuple<int,string>(depth,String.Empty));
            else
                realReset(depth,String.Empty);
        }

        /// <summary>
        /// Real reset method, after being filtered by the public mask.
        /// </summary>
        /// <param name="depth">The level of depth in the condition tree, if -1 it means any.</param>
        /// <param name="condition">The condition for the level of depth, if String.Empty it means any.</param>
        private void realReset(int depth, String condition)
        {
            if (depth == -1)
            {
                conditions = new List<Dictionary<string, List<Tuple<Delegate, object[]>>>>();
                safetyBox = new List<Dictionary<string, object>>();
                return;
            }

            if (condition == String.Empty)
            {
                conditions[depth] = new Dictionary<string, List<Tuple<Delegate, object[]>>>();
                safetyBox[depth] = new Dictionary<string, object>();
                return;
            }

            conditions[depth][condition] = new List<Tuple<Delegate, object[]>>();
            safetyBox[depth][condition] = null;
        }

        #endregion
    }
}
