import React from 'react'
import './Footer.css'
import { assets } from '../../assets/assets'

const Footer = () => {
  return (
    <div className='footer' id='footer'>
        <div className="footer-content">
            <div className="footer-content-left">
                <h2>Bella Restaurant</h2>
            <p>Lorem ipsum dolor sit amet consectetur adipisicing elit. Amet, alias? Dolores, similique eos sunt voluptatum at beatae repudiandae sed id aliquid, accusantium dolor provident voluptates explicabo adipisci nulla inventore ducimus.</p>
            <div className="footer-social-icons">
                <img src={assets.facebook_icon} alt="" />
                <img src={assets.twitter_icon} alt="" />
                <img src={assets.linkedin_icon} alt="" />
            </div>
            </div>
            <div className="footer-content-center">
                <h2>COMPANY</h2>
                <ul>
                    <li>Home</li>
                    <li>About Us</li>
                    <li>Delivery</li>
                    <li>Privacy Policy</li>
                </ul>
            </div>
            <div className="footer-content-right">
            <h2>GET IN TOUCH</h2>
            <ul>
                <li>+381-62-189-848</li>
                <li>contact@bella.com</li>
            </ul>
        </div>
        </div>
           <hr />
        <p className="footer-copyright">Copyright 2024 © Bella.com - All right Reserved.</p>
    </div>
  )
}

export default Footer
