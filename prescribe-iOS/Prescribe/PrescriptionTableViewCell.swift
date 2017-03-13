//
//  PrescriptionTableViewCell.swift
//  Prescribe
//
//  Created by Sagar Punhani on 11/12/16.
//  Copyright © 2016 Sagar Punhani. All rights reserved.
//

import UIKit

class PrescriptionTableViewCell: UITableViewCell {
    
    @IBOutlet weak var pillImage: UIImageView!
    
    @IBOutlet weak var pillLabel: UILabel!
    
    @IBOutlet weak var takenLabel: UILabel!
    
    @IBOutlet weak var typeLabel: UILabel!

    override func awakeFromNib() {
        super.awakeFromNib()
        takenLabel.clipsToBounds = true
        takenLabel.layer.cornerRadius = 20
        pillImage.layer.borderWidth = 1.0
        pillImage.layer.cornerRadius = 10.0
        pillImage.clipsToBounds = true
        
        // Initialization code
    }

    override func setSelected(_ selected: Bool, animated: Bool) {
        super.setSelected(selected, animated: animated)

        // Configure the view for the selected state
    }
    
    func setUp(pill: Prescription) {
        pillImage.image = pill.image
        pillLabel.text = pill.name
        switch pill.taken {
        case 0: takenLabel.backgroundColor = UIColor.red
        case 1: takenLabel.backgroundColor = UIColor.green
        default: break
        }
        typeLabel.text = pill.time
    }

}
