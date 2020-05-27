import React from "react";
import { connect } from "react-redux";

class Item extends React.Component {
    render() {
        const { route } = this.props;
        let isSelected = route.id === (this.props.knownRoute && this.props.knownRoute.id)

        return (
            <li 
                key={route.id} 
                className={`list-group-item d-flex justify-content-between lh-condensed ${isSelected ? "bg-light" : ""} `}
                onClick={() => this.props.onChange(route)}   
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
