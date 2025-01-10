import React, { useState, useContext, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Navbar.css';
import { assets } from '../../assets/assets';
import { Link } from 'react-router-dom';
import { StoreContext } from '../../context/StoreContext';

const Navbar = ({ setShowLogin }) => {
  const [menu, setMenu] = useState("menu");
  const { getTotalCartAmount, token, setToken } = useContext(StoreContext);
  const navigate = useNavigate();

 
  useEffect(() => {
    const storedToken = localStorage.getItem("authToken");
    if (storedToken) {
      setToken(storedToken); 
    }
  }, [setToken]);

  const handleLogout = () => {
    localStorage.removeItem("authToken");
    localStorage.removeItem("userId");
    setToken(""); 
    navigate("/"); 
  };

  return (
    <div className='navbar'>
      <div className="navbar-left">
        <Link to='/'>
          <img src={assets.logo} alt="Logo" className="logo" />
        </Link>
        <h2>Bella Restaurant</h2>
      </div>
      <ul className="navbar-menu">
        <Link to='/' onClick={() => setMenu("home")} className={menu === "home" ? "active" : ""}>Home</Link>
        <a href='#explore-menu' onClick={() => setMenu("menu")} className={menu === "menu" ? "active" : ""}>Menu</a>
        <a href='#footer' onClick={() => setMenu("contact-us")} className={menu === "contact-us" ? "active" : ""}>Contact Us</a>
      </ul>
      <div className="navbar-right">
        <div className="navbar-search-icon">
          <Link to='/cart'>
            <img src={assets.basket_icon} alt="Cart" />
          </Link>
          <div className={getTotalCartAmount() > 0 ? "dot" : ""}></div>
        </div>

        {!token ? (
          <button onClick={() => setShowLogin(true)}>Sign In</button>
        ) : (
          <div className='navbar-profile'>
            <img src={assets.profile_icon} alt="Profile" />
            <ul className="nav-profile-dropdown">
              <li>
                <img src={assets.bag_icon} alt="Orders" />
                <p>Orders</p>
              </li>
              <hr />
              <li>
                <img src={assets.logout_icon} alt="Logout" />
                <p onClick={handleLogout}>Logout</p>
              </li>
            </ul>
          </div>
        )}
      </div>
    </div>
  );
};

export default Navbar;
