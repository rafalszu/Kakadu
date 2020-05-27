import React from "react";
import { connect } from "react-redux";
import { knownRouteActions } from '../../_actions'

class Item extends React.Component {
    constructor(props) {
        super(props);

        this.handleRouteSelected = this.handleRouteSelected.bind(this);
    }
    
    handleRouteSelected(route) {
        this.props.dispatch(knownRouteActions.selectKnownRoute(route));
    }

    render() {
        const { route } = this.props;
        let isSelected = route.id === (this.props.knownRoute && this.props.knownRoute.id)

        return (
            <li 
                key={route.id} 
                style={{cursor: "pointer"}}
                className={`list-group-item d-flex justify-content-between lh-condensed ${isSelected ? "bg-light" : ""} `}
                onClick={() => this.handleRouteSelected(route)}   
            >
                <span>{route.relativeUrl}</span>
                <span className="text-muted">{route.methodName}</span>
            </li>
        )
    }
}

function mapStateToProps(state) {
    const { knownRoute } = state;
    return {
        knownRoute
    }
}

const connectedKnownRouteItemPage = connect(mapStateToProps)(Item);
export { connectedKnownRouteItemPage as Item };
