﻿using EA4S.Core;
using UnityEngine;

namespace EA4S.Rewards
{
    public static class ModelsManager
    {
        #region API

        public static GameObject MountModel(string _id, Transform _parent, ModelType _type = ModelType.AnturaForniture)
        {
            GameObject rewardModel = GetObject(_type, _id);
            rewardModel.transform.SetParent(_parent, false);
            return rewardModel;
        }

        public static GameObject MountModel(string _id, Transform _parent, MaterialPair _materialPair,
            ModelType _type = ModelType.AnturaForniture)
        {
            CleanTranformChildren(_parent);
            GameObject returnObject = MountModel(_id, _parent, _type);
            //Reward actualReward = RewardSystemManager.GetRewardById(_id);
            SwitchMaterial(returnObject, _materialPair);
            return returnObject;
        }

        public static MaterialPair SwitchMaterial(GameObject _gameObject, MaterialPair _materialPair)
        {
            if (_materialPair.Material1 == null || _materialPair.Material2 == null)
                return _materialPair;
            foreach (var color in _gameObject.GetComponentsInChildren<MeshRenderer>()) {
                if (color.name == "color_1") {
                    color.materials = new Material[] {_materialPair.Material1};
                } else if (color.name == "color_2") {
                    color.materials = new Material[] {_materialPair.Material2};
                }
            }
            return _materialPair;
        }

        public static void CleanTranformChildren(Transform _parent)
        {
            for (int i = _parent.childCount - 1; i >= 0; --i) {
                var child = _parent.GetChild(i).gameObject;
                GameObject.Destroy(child);
            }
        }
        #endregion

        #region internal functionalities 

        static GameObject GetObject(ModelType _type, string _id)
        {
            string resourceToLoadPath;
            switch (_type) {
                case ModelType.AnturaForniture:
                    resourceToLoadPath = RewardSystemManager.ANTURA_REWARDS_PREFABS_PATH;
                    break;
                default:
                    return null;
            }

            GameObject model = GameObject.Instantiate(Resources.Load(resourceToLoadPath + _id)) as GameObject;

            return model;
        }

        #endregion


        public enum ModelType
        {
            AnturaForniture,
        }
    }
}