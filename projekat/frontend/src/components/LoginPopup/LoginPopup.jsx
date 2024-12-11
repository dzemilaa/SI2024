import React, { useState } from 'react';
import './LoginPopup.css'
import { assets } from '../../assets/assets'

const LoginPopup = ({setShowLogin}) => {

    const [currState, setCurrState] = useState("Login")

  return (
    <div className='login-popup'>
        <form className='login-popup-container'>
        <div className="login-popup-title">
            <h2>{currState}</h2>
            <img onClick={()=> setShowLogin(false) } src={assets.cross_icon} alt="" />
        </div>
        <div className="login-popup-inputs">
            {currState ==="Login"?<></>:  <input type="text" placeholder='Unesite ime' required />}
            <input type="email" placeholder='Unesite e-mail' required />
            <input type="password" placeholder='Unesite lozinku' required />
        </div>
        <button> {currState === "Sign Up"?"Create account":"Login"}</button>
    <div className="login-popup-condition">
        <input type="checkbox" required />
        <p>Prihvatam uslove korišćenja i politiku privatnosti.</p>
    </div>
    {currState === "Login" 
    ?  <p>Kreirajte novi nalog? <span onClick={()=>setCurrState("Sign Up")}>Kliknite ovde</span></p>
    : <p>Imate već nalog? <span onClick={()=>setCurrState("Login")}>Prijavite se ovde</span></p>}
   
   
    </form>
    </div>
  )
}

export default LoginPopup
