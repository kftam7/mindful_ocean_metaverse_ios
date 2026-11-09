using System;
using Auki.ConjureKit;
using Auki.ConjureKit.Domains;
using Auki.ConjureKit.Manna;
using UnityEngine;

public class DomainEditorDebug
{
        public void MockScan(string id, IConjureKit conjureKit, Action<StaticLighthouseData> onSuccess, Action<string> onFailed)
        {
            conjureKit.GetLighthouseWithDomains(id, OrganizationSource.Any, lighthouse =>
            {
                var data = new StaticLighthouseData();
                data.poses = new LighthousePose[lighthouse.DomainsCached.Count];

                var id = 0;
                foreach (var l in lighthouse.DomainsCached)
                {
                    var lighthousePose = new LighthousePose();
                    lighthousePose.domainId = l.Value.Domain.Id;
                    lighthousePose.domainName = l.Value.Domain.Name;
                    lighthousePose.placedAt = l.Value.AddedToDomainAt;
                    l.Value.GetPose(
                        pose => {
                            lighthousePose.px = pose.Position.x;
                            lighthousePose.py = pose.Position.y;
                            lighthousePose.pz = pose.Position.z;
                            lighthousePose.rx = pose.Rotation.x;
                            lighthousePose.ry = pose.Rotation.y;
                            lighthousePose.rz = pose.Rotation.z;
                            lighthousePose.rw = pose.Rotation.w;
                            data.poses[id] = lighthousePose;
            
                            if (id == lighthouse.DomainsCached.Count - 1) {
                                onSuccess?.Invoke(data);
                            }
                            id++;
                        }, 
                       error =>
                       {
                           Debug.LogWarning(error);
                           data.poses[id] = new LighthousePose();
                           if (id == lighthouse.DomainsCached.Count - 1) {
                               onSuccess?.Invoke(data);
                           }
                           id++;
                       },
                        false
                    );
                }
            }, error =>
            {
                Debug.LogError(error);
                onFailed?.Invoke(error);
            });
        }
}
