using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PermissionMission
{
    bool ShowPermissionPopup();
    bool ShowPermissionFailWarning();
    bool GetPermission();
}