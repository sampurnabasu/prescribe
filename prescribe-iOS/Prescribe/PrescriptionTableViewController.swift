//
//  PrescriptionTableViewController.swift
//  Prescribe
//
//  Created by Sagar Punhani on 11/12/16.
//  Copyright © 2016 Sagar Punhani. All rights reserved.
//

import UIKit
import Firebase

struct Prescription {
    let name: String
    let dosage: Int
    let taken: Int
    let time: String
    let type: Int
    let image: UIImage
    let des: String
    init(name: String, dosage: Int, taken: Int, time: String, type: Int, image: UIImage, des: String) {
        self.name = name
        self.dosage = dosage
        self.taken = taken
        self.time = time
        self.type = type
        self.image = image
        self.des = des
    }
}

struct Constants {
    static let username = "David"
}

class PrescriptionTableViewController: UITableViewController {
    
    var pills = [Prescription]()
    
    var ref: FIRDatabaseReference!
    
    var first = true

    override func viewDidLoad() {
        super.viewDidLoad()
        ref = FIRDatabase.database().reference()
        ref.child("Users").observeSingleEvent(of: .value, with: { (snapshot) in
            // Get user value
            let value = snapshot.value as! NSDictionary
            let user = value[Constants.username] as! NSDictionary
            let prescription = user["Prescription"] as! NSMutableArray
            prescription.removeObject(at: 0)
            for item in prescription {
                let pill = item as! NSDictionary
                self.updateUI(pill: pill)
            }
            
            // ...
        }) { (error) in
            print(error.localizedDescription)
        }
        
        
        
        ref.child("Users").child(Constants.username).child("Prescription").observe(FIRDataEventType.value, with: { (snapshot) in
            let postDict = snapshot.value as! NSMutableArray
            postDict.removeObject(at: 0)
            if !self.first {
                self.pills.removeAll()
                for item in postDict {
                    let pill = item as! NSDictionary
                    self.updateUI(pill: pill)
                }
            }
            else {
                self.first = !self.first
            }
            
        })
        
        
        
        

        // Uncomment the following line to preserve selection between presentations
        // self.clearsSelectionOnViewWillAppear = false

        // Uncomment the following line to display an Edit button in the navigation bar for this view controller.
        // self.navigationItem.rightBarButtonItem = self.editButtonItem()
    }
    
    func updateUI(pill: NSDictionary) {
        let des = pill["Description"] as! String
        let name = pill["Name"] as! String
        let taken = pill["Taken"] as! Int
        let time = pill["Time"] as! String
        let image = UIImage(named: name)
        let type = pill["Type"] as! Int
        let dosage = pill["Dosage"] as! Int
        let p1 = Prescription(name: name, dosage: dosage, taken: taken, time: time, type: type, image: image!, des: des)
        self.pills.append(p1)
        self.tableView.reloadData()

    }

    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }

    // MARK: - Table view data source

    override func numberOfSections(in tableView: UITableView) -> Int {
        // #warning Incomplete implementation, return the number of sections
        return 1
    }

    override func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        // #warning Incomplete implementation, return the number of rows
        return pills.count
    }

    
    override func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let cell = tableView.dequeueReusableCell(withIdentifier: "prescription", for: indexPath) as! PrescriptionTableViewCell
        
        cell.setUp(pill: pills[indexPath.row])
        
        // Configure the cell...

        return cell
    }
    
    override func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        performSegue(withIdentifier: "pill", sender: pills[indexPath.row])
    }
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        if let ptvc = segue.destination as? PillTableViewController {
            ptvc.pill = sender as? Prescription
        }
    }
    

    /*
    // Override to support conditional editing of the table view.
    override func tableView(_ tableView: UITableView, canEditRowAt indexPath: IndexPath) -> Bool {
        // Return false if you do not want the specified item to be editable.
        return true
    }
    */

    /*
    // Override to support editing the table view.
    override func tableView(_ tableView: UITableView, commit editingStyle: UITableViewCellEditingStyle, forRowAt indexPath: IndexPath) {
        if editingStyle == .delete {
            // Delete the row from the data source
            tableView.deleteRows(at: [indexPath], with: .fade)
        } else if editingStyle == .insert {
            // Create a new instance of the appropriate class, insert it into the array, and add a new row to the table view
        }    
    }
    */

    /*
    // Override to support rearranging the table view.
    override func tableView(_ tableView: UITableView, moveRowAt fromIndexPath: IndexPath, to: IndexPath) {

    }
    */

    /*
    // Override to support conditional rearranging of the table view.
    override func tableView(_ tableView: UITableView, canMoveRowAt indexPath: IndexPath) -> Bool {
        // Return false if you do not want the item to be re-orderable.
        return true
    }
    */

    /*
    // MARK: - Navigation

    // In a storyboard-based application, you will often want to do a little preparation before navigation
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        // Get the new view controller using segue.destinationViewController.
        // Pass the selected object to the new view controller.
    }
    */

}
