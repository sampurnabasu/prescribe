//
//  SettingsViewController.swift
//  Prescribe
//
//  Created by Sagar Punhani on 11/11/16.
//  Copyright © 2016 Sagar Punhani. All rights reserved.
//

import UIKit
import BRYXBanner
import HealthKit
import Firebase

class SettingsViewController: UIViewController {
    @IBOutlet weak var heartImage: UIImageView!
    
    @IBOutlet weak var weightImage: UIImageView!
    
    @IBOutlet weak var sleepImage: UIImageView!
    
    @IBOutlet weak var heartLabel: UILabel!
    
    @IBOutlet weak var weightLabel: UILabel!
    
    @IBOutlet weak var sleepLabel: UILabel!
    
    @IBOutlet weak var nameLabel: UILabel!
    
    
    @IBOutlet weak var firstNotifyButton: UIButton!
    
    
    @IBOutlet weak var secondNotifyButton: UIButton!
    
    @IBOutlet weak var thirdNotifyButton: UIButton!
    
    var ref: FIRDatabaseReference?

    let color = UIColor(colorLiteralRed: 104/255.0, green: 170/255.0, blue: 235/255.0, alpha: 1.0).cgColor
    
    
    @IBAction func firstNotify(_ sender: UIButton) {
        resetColor()
        firstNotifyButton.setTitleColor(.black, for: .normal)
        firstNotifyButton.layer.borderColor = color
    }
    
    @IBAction func secondNotify(_ sender: UIButton) {
        resetColor()
        secondNotifyButton.setTitleColor(.black, for: .normal)
        secondNotifyButton.layer.borderColor = color
        let banner = Banner(title: "Pills are Ready!!", subtitle: "Please go to your nearest Prescribe", image: nil, backgroundColor: .blue, didTapBlock: nil)
        banner.dismissesOnTap = true
        banner.show(duration: 3.0)
 
    }
    
    @IBAction func thirdNotify(_ sender: UIButton) {
        resetColor()
        thirdNotifyButton.setTitleColor(.black, for: .normal)
        thirdNotifyButton.layer.borderColor = color
    }
    
    func resetColor() {
        firstNotifyButton.setTitleColor(.lightGray, for: .normal)
        secondNotifyButton.setTitleColor(.lightGray, for: .normal)
        thirdNotifyButton.setTitleColor(.lightGray, for: .normal)
        firstNotifyButton.layer.borderColor = UIColor.lightGray.cgColor
        secondNotifyButton.layer.borderColor = UIColor.lightGray.cgColor
        thirdNotifyButton.layer.borderColor = UIColor.lightGray.cgColor
    }
    
    

    override func viewDidLoad() {
        super.viewDidLoad()
        ref = FIRDatabase.database().reference()
        var width: CGFloat = 5.0
        var radius: CGFloat = 50
        heartImage.layer.borderWidth = width
        heartImage.layer.borderColor = UIColor(colorLiteralRed: 231/255.0, green: 76/255.0, blue: 60/255.0, alpha: 1.0).cgColor
        heartImage.layer.cornerRadius = radius
        
        weightImage.layer.borderWidth = width
        weightImage.layer.borderColor = UIColor(colorLiteralRed: 243/255.0, green: 156/255.0, blue: 18/255.0, alpha: 1.0).cgColor
        weightImage.layer.cornerRadius = radius
        
        sleepImage.layer.borderWidth = width
        sleepImage.layer.borderColor = UIColor(colorLiteralRed: 52/255.0, green: 152/255.0, blue: 219/255.0, alpha: 1.0).cgColor

        sleepImage.layer.cornerRadius = radius
        
        width = 2.0
        radius = 35.0
        nameLabel.layer.borderWidth = width
        nameLabel.layer.borderColor = UIColor.green.cgColor
        nameLabel.layer.cornerRadius = radius
        nameLabel.textColor = UIColor.black
        
        firstNotifyButton.layer.borderWidth = width
        firstNotifyButton.layer.borderColor = self.color
        firstNotifyButton.layer.cornerRadius = radius
        firstNotifyButton.setTitleColor(.black, for: .normal)
        
        secondNotifyButton.layer.borderWidth = width
        secondNotifyButton.layer.borderColor = UIColor.lightGray.cgColor
        secondNotifyButton.layer.cornerRadius = radius
        secondNotifyButton.setTitleColor(.lightGray, for: .normal)
        
        thirdNotifyButton.layer.borderWidth = width
        thirdNotifyButton.layer.borderColor = UIColor.lightGray.cgColor
        thirdNotifyButton.layer.cornerRadius = radius
        thirdNotifyButton.setTitleColor(.lightGray, for: .normal)
        
        updateUI()

        // Do any additional setup after loading the view.
    }
    
    func updateUI() {
        // 1. Set the types you want to read from HK Store
        let healthKitTypesToRead:Set<HKObjectType> = [HKObjectType.characteristicType(forIdentifier: .dateOfBirth)!,HKObjectType.quantityType(forIdentifier: .bodyMass)!,HKObjectType.quantityType(forIdentifier: .height)!]
        
        let healthKitStore: HKHealthStore = HKHealthStore()
        
        healthKitStore.requestAuthorization(toShare: nil, read: healthKitTypesToRead) { (success, error) -> Void in
            // 1. Request birthday and calculate age
            do {
                let birthDay = try healthKitStore.dateOfBirthComponents()
                if error != nil {
                    print("Error reading Birthday: \(error)")
                }
                let age = Date().years(from: birthDay.date!)
                self.heartLabel.text = age.description + " yrs"
                self.ref?.child("Users").child(Constants.username).child("Age").setValue(age)
            }
            catch _ {
                
            }
            
            // weight type
            let weightSampleType = HKSampleType.quantityType(forIdentifier: .bodyMass)!
            
            // run the query
            let weightQuery = HKSampleQuery(sampleType: weightSampleType, predicate: nil, limit: 1, sortDescriptors: nil) {
                
                (query, results, error) in
                
                // error with query
                if error != nil {
                    return
                }
                
                // check for valid results
                guard let results = results else {
                    print("No results of query")
                    return
                }
                
                // make sure there is at least one result to output
                if results.count == 0 {
                    print("Zero samples")
                    return
                }
                
                // extract the one sample
                guard let bodymass = results[0] as? HKQuantitySample else {
                    print("Type problem with weight")
                    return
                }
                let weight = bodymass.quantity.doubleValue(for: .pound()).roundTo(places: 1)
                self.weightLabel.text = weight.description + " lbs"
                self.ref?.child("Users").child(Constants.username).child("Weight").setValue(weight)
                
            }
            
            // execute the query
            healthKitStore.execute(weightQuery)
        }
        
    }


    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }
    

    /*
    // MARK: - Navigation

    // In a storyboard-based application, you will often want to do a little preparation before navigation
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        // Get the new view controller using segue.destinationViewController.
        // Pass the selected object to the new view controller.
    }
    */

}

extension Date {
    /// Returns the amount of years from another date
    func years(from date: Date) -> Int {
        return Calendar.current.dateComponents([.year], from: date, to: self).year ?? 0
    }
    /// Returns the amount of months from another date
    func months(from date: Date) -> Int {
        return Calendar.current.dateComponents([.month], from: date, to: self).month ?? 0
    }
    /// Returns the amount of weeks from another date
    func weeks(from date: Date) -> Int {
        return Calendar.current.dateComponents([.weekOfYear], from: date, to: self).weekOfYear ?? 0
    }
    /// Returns the amount of days from another date
    func days(from date: Date) -> Int {
        return Calendar.current.dateComponents([.day], from: date, to: self).day ?? 0
    }
    /// Returns the amount of hours from another date
    func hours(from date: Date) -> Int {
        return Calendar.current.dateComponents([.hour], from: date, to: self).hour ?? 0
    }
    /// Returns the amount of minutes from another date
    func minutes(from date: Date) -> Int {
        return Calendar.current.dateComponents([.minute], from: date, to: self).minute ?? 0
    }
    /// Returns the amount of seconds from another date
    func seconds(from date: Date) -> Int {
        return Calendar.current.dateComponents([.second], from: date, to: self).second ?? 0
    }
    /// Returns the a custom time interval description from another date
    func offset(from date: Date) -> String {
        if years(from: date)   > 0 { return "\(years(from: date))y"   }
        if months(from: date)  > 0 { return "\(months(from: date))M"  }
        if weeks(from: date)   > 0 { return "\(weeks(from: date))w"   }
        if days(from: date)    > 0 { return "\(days(from: date))d"    }
        if hours(from: date)   > 0 { return "\(hours(from: date))h"   }
        if minutes(from: date) > 0 { return "\(minutes(from: date))m" }
        if seconds(from: date) > 0 { return "\(seconds(from: date))s" }
        return ""
    }
}

extension Double {
    /// Rounds the double to decimal places value
    func roundTo(places:Int) -> Double {
        let divisor = pow(10.0, Double(places))
        return (self * divisor).rounded() / divisor
    }
}
