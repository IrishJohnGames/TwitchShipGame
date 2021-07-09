using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameManager : ManagerBase<NameManager>
{
    List<string> CrewRoleNames = new List<string>() {
        "Captain        ",
        "Quartermaster  ",
        "First Mate     ",
        "Second Mate    ",
        "Third Mate     ",
        "Boatswain      ",
        "Cabin Boy      ",
        "Carpenter      ",
        "Gunner         ",
        "Master-at-Arms ",
        "Navigator      ",
        "Helmsman       ",
        "Powder Monkey  ",
        "Striker        ",
        "Surgeon        ",
        "Deckhand       ",
        "Cooper         ",
        "Cook           "
    };

    public string GetCrewRoleName(int index) => CrewRoleNames[index];
}