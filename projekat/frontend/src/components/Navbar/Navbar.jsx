import React, { useState } from 'react'
import './Navbar.css'
import {assets} from '../../assets/assets'

const Navbar = () => {

    const [menu, setMenu] = useState("home")

  return (
    <div className='navbar'>
      <div className="navbar-left">
      <img src={assets.logo} alt="" className="logo" />
      <h2>Bella Restaurant</h2>
      </div>
      <ul className="navbar-menu">
        <li onClick={()=>setMenu("home")} className={menu==="home"?"active":""}>Home</li>
        <li onClick={()=>setMenu("menu")} className={menu==="menu"?"active":""}>Menu</li>
        <li onClick={()=>setMenu("contact-us")} className={menu==="contact-us"?"active":""}>Contact Us</li>
      </ul>
      <div className="navbar-right">
       
        <div className="navbar-search-icon">
            <img src={assets.basket_icon} alt="" />
            <div className="dot"></div>
        </div>
        <button>Sign In</button>
      </div>
    </div>
  )
}

export default Navbar
