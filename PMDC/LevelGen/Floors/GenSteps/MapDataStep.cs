﻿using System;
using RogueEssence.Dungeon;
using System.Collections.Generic;
using RogueElements;
using RogueEssence.Data;
using RogueEssence.LevelGen;
using System.Text;
using RogueEssence.Dev;
using PMDC.Dungeon;

namespace PMDC.LevelGen
{
    /// <summary>
    /// Sets various attributes about the map.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MapDataStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        /// <summary>
        /// The default map music.
        /// </summary>
        [Music(0)]
        public string Music;

        /// <summary>
        /// How many turns the player can spend on the map before an instant game over.
        /// </summary>
        public int TimeLimit;

        /// <summary>
        /// The darkness level for map exploration.
        /// </summary>
        public Map.SightRange TileSight;

        /// <summary>
        /// The darkness level for character viewing.
        /// </summary>
        public Map.SightRange CharSight;

        public MapDataStep()
        {
            Music = "";
        }
        public MapDataStep(string music, int timeLimit, Map.SightRange tileSight, Map.SightRange charSight)
        {
            Music = music;
            TimeLimit = timeLimit;
            TileSight = tileSight;
            CharSight = charSight;
        }

        public override void Apply(T map)
        {
            map.Map.Music = Music;

            if (TimeLimit > 0)
            {
                MapStatus timeStatus = new MapStatus(22);
                timeStatus.LoadFromData();
                MapCountDownState timeState = timeStatus.StatusStates.GetWithDefault<MapCountDownState>();
                timeState.Counter = TimeLimit;
                map.Map.Status.Add(22, timeStatus);
            }


            map.Map.TileSight = TileSight;
            map.Map.CharSight = CharSight;
        }


        public override string ToString()
        {
            return String.Format("{0}: Time:{1} Song:{2} TileSight:{3} CharSight:{4}", this.GetType().Name, TimeLimit, Music, TileSight, CharSight);
        }
    }

    /// <summary>
    /// Adds a map status that is considered the "default" for that map.
    /// The map will always revert back to this status even if replaced (it will wait for the replacing status to run out).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class DefaultMapStatusStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        /// <summary>
        /// The map status used to set the default map status.
        /// </summary>
        [DataType(0, DataManager.DataType.MapStatus, false)]
        public int SetterID;

        /// <summary>
        /// The possible default map statuses.
        /// </summary>
        [DataType(1, DataManager.DataType.MapStatus, false)]
        public int[] DefaultMapStatus;

        public DefaultMapStatusStep()
        {
            DefaultMapStatus = new int[1] { 0 };
        }
        public DefaultMapStatusStep(int statusSetter, params int[] defaultStatus)
        {
            SetterID = statusSetter;
            DefaultMapStatus = defaultStatus;
        }

        public override void Apply(T map)
        {
            int chosenStatus = DefaultMapStatus[map.Rand.Next(DefaultMapStatus.Length)];
            MapStatus statusSetter = new MapStatus(SetterID);
            statusSetter.LoadFromData();
            MapIndexState indexState = statusSetter.StatusStates.GetWithDefault<MapIndexState>();
            indexState.Index = chosenStatus;
            map.Map.Status.Add(SetterID, statusSetter);
        }


        public override string ToString()
        {
            string status = String.Format("{0} Choices", DefaultMapStatus.Length);
            if (status.Length == 1)
                status = DataManager.Instance.DataIndices[DataManager.DataType.MapStatus].Entries[status[0]].Name.ToLocal();
            return String.Format("{0}: {1}", this.GetType().Name, status);
        }
    }


    /// <summary>
    /// Adds a map status to the map, with the specified MapStatusStates
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class StateMapStatusStep<T> : GenStep<T> where T : BaseMapGenContext
    {
        public int MapStatus;
        public StateCollection<MapStatusState> States;

        public StateMapStatusStep()
        {
            States = new StateCollection<MapStatusState>();
        }
        public StateMapStatusStep(int mapStatus, MapStatusState state) : this()
        {
            MapStatus = mapStatus;
            States.Set(state);
        }

        public override void Apply(T map)
        {
            MapStatus status = new MapStatus(MapStatus);
            status.LoadFromData();
            foreach(MapStatusState state in States)
                status.StatusStates.Set((MapStatusState)state.Clone());
            map.Map.Status.Add(MapStatus, status);
        }
    }
}
