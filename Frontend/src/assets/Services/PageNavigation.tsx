import { NavLink } from 'react-router-dom'

/**
 * Navigation component for the main application pages
 */
function PageNavigation() {
    return (
        <nav>
            <NavLink to="/homepage" >HomePage</NavLink>
            <span className="separator">|</span>
        </nav>
    )
}

export default PageNavigation