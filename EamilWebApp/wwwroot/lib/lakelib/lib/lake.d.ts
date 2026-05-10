import EventEmitter from 'eventemitter3';
import { LocalizedString, RequiredParams } from 'typesafe-i18n';

type KeyValue = Record<string, string>;

type ContentStyleValue = string | string[] | RegExp;
type ContentStyle = Record<string, ContentStyleValue>;
type ContentAttributeValue = string | string[] | RegExp | ContentStyle;
type ContentAttribute = Record<string, ContentAttributeValue>;
type ContentRules = Record<string, string | ContentAttribute>;

interface EventItem {
    type: string;
    listener: EventListener;
}
/**
 * The Nodes interface represents a collection of the nodes.
 * It is similar to jQuery, but its implementation is much simpler.
 * Its methods can be considered aliases of native DOM interfaces, designed to simplify DOM manipulation.
 */
declare class Nodes {
    /**
     * A list of native nodes.
     */
    private readonly nodeList;
    /**
     * The number of nodes in the Nodes object.
     */
    readonly length: number;
    constructor(node?: Node | Node[] | null);
    /**
     * The unique ID of the first node.
     */
    get id(): number;
    /**
     * The name of the first node.
     */
    get name(): string;
    /**
     * A boolean value indicating whether the first node is an element.
     */
    get isElement(): boolean;
    /**
     * A boolean value indicating whether the first node is a text.
     */
    get isText(): boolean;
    /**
     * A boolean value indicating whether the first node is a block.
     */
    get isBlock(): boolean;
    /**
     * A boolean value indicating whether the first node is a mark.
     */
    get isMark(): boolean;
    /**
     * A boolean value indicating whether the first node is a void element that cannot have any child nodes.
     */
    get isVoid(): boolean;
    /**
     * A boolean value indicating whether the first node is a heading.
     */
    get isHeading(): boolean;
    /**
     * A boolean value indicating whether the first node is a list.
     */
    get isList(): boolean;
    /**
     * A boolean value indicating whether the first node is a table.
     */
    get isTable(): boolean;
    /**
     * A boolean value indicating whether the first node is a bookmark element.
     */
    get isBookmark(): boolean;
    /**
     * A boolean value indicating whether the first node is a box element.
     */
    get isBox(): boolean;
    /**
     * A boolean value indicating whether the first node is an inline box element.
     */
    get isInlineBox(): boolean;
    /**
     * A boolean value indicating whether the first node is a block box element.
     */
    get isBlockBox(): boolean;
    /**
     * A boolean value indicating whether the first node is a contenteditable element where users can edit the content.
     */
    get isContainer(): boolean;
    /**
     * A boolean value indicating whether the first node does not have an ancestor element which contenteditable attribute is true.
     */
    get isOutside(): boolean;
    /**
     * A boolean value indicating whether the first node has an ancestor element which contenteditable attribute is true.
     */
    get isInside(): boolean;
    /**
     * A boolean value indicating whether the first node's parent element is an element which contenteditable attribute is true.
     */
    get isTopInside(): boolean;
    /**
     * A boolean value indicating whether the first node is editable.
     */
    get isContentEditable(): boolean;
    /**
     * A boolean value indicating whether the first node is indivisible.
     */
    get isIndivisible(): boolean;
    /**
     * A boolean value indicating whether the first node is empty.
     */
    get isEmpty(): boolean;
    /**
     * Returns a native node at the specified index.
     */
    get(index: number): Node;
    /**
     * Returns all native nodes.
     */
    getAll(): Node[];
    /**
     * Returns a new Nodes object that includes only the node at the specified index.
     */
    eq(index: number): Nodes;
    /**
     * Executes a provided function once for each node.
     */
    each(callback: (node: Node, index: number) => boolean | void): this;
    /**
     * Executes a provided function once for each element.
     */
    eachElement(callback: (element: Element, index: number) => boolean | void): this;
    /**
     * Returns a new Nodes object with the nodes in reversed order.
     */
    reverse(): Nodes;
    /**
     * Tests whether the first node would be selected by the specified CSS selector.
     */
    matches(selector: string): boolean;
    /**
     * Returns a boolean value indicating whether the given node is a descendant of the first node,
     * that is the node itself, one of its direct children (childNodes), one of the children's direct children, and so on.
     */
    contains(otherNode: Nodes): boolean;
    /**
     * Returns a boolean value indicating whether the first node and a given node are siblings.
     */
    isSibling(otherNode: Nodes): boolean;
    /**
     * Returns the descendants of the first node that match the specified CSS selector or node path.
     */
    find(selector: string | NodePath): Nodes;
    /**
     * Traverses the first node and its parents (heading toward the document root) until it finds an element that matches the specified CSS selector.
     */
    closest(selector: string): Nodes;
    /**
     * Traverses the first node and its parents until it finds a block element.
     */
    closestBlock(): Nodes;
    /**
     * Traverses the first node and its parents until it finds an operable block.
     */
    closestOperableBlock(): Nodes;
    /**
     * Traverses the first node and its parents until it finds a div element which contenteditable attribute is true.
     */
    closestContainer(): Nodes;
    /**
     * Traverses the first node and its parents until it finds an element which can scroll.
     */
    closestScroller(): Nodes;
    /**
     * Returns the parent of the first node.
     */
    parent(): Nodes;
    /**
     * Returns the immediately preceding sibling of the first node.
     */
    prev(): Nodes;
    /**
     * Returns the immediately following sibling of the first node.
     */
    next(): Nodes;
    /**
     * Returns the first child of the first node.
     */
    first(): Nodes;
    /**
     * Returns the last child of the first node.
     */
    last(): Nodes;
    /**
     * Returns a number indicating the position of the first node relative to its sibling nodes.
     */
    index(): number;
    /**
     * Returns the path of the first node.
     */
    path(): NodePath;
    /**
     * Returns a list which contains all of the child nodes of the first node.
     */
    children(): Nodes[];
    /**
     * Returns a generator that iterates over the descendants of the first node.
     */
    getWalker(): Generator<Nodes>;
    /**
     * Sets up an event listener for each element.
     */
    on(type: string, listener: EventListener): this;
    /**
     * Removes event listeners previously registered with on() method.
     */
    off(type?: string, listener?: EventListener): this;
    /**
     * Executes all event listeners attached to all nodes for the given event type.
     */
    emit(type: string, event?: Event): this;
    /**
     * Returns all event listeners attached to the node at the specified index.
     */
    getEventListeners(index: number): EventItem[];
    /**
     * Sets focus on the specified node, if it can be focused.
     */
    focus(): this;
    /**
     * Removes focus from the specified node.
     */
    blur(): this;
    /**
     * Returns a copy of the first node. If deep is true, the copy also includes the node's descendants.
     */
    clone(deep?: boolean): Nodes;
    /**
     * Returns a boolean value indicating whether the first node has the specified attribute or not.
     */
    hasAttr(attributeName: string): boolean;
    /**
     * Returns the value of the specified attribute from the first node, or sets the values of attributes for all elements.
     */
    attr(attributeName: string): string;
    attr(attributeName: string, value: string): this;
    attr(attributeName: KeyValue): this;
    /**
     * Removes the attribute with the specified name from every element.
     */
    removeAttr(attributeName: string): this;
    /**
     * Returns a boolean value indicating whether the first node has the specified class or not.
     */
    hasClass(className: string): boolean;
    /**
     * Adds the given class to every element.
     */
    addClass(className: string | string[]): this;
    /**
     * Removes the given class from every element.
     */
    removeClass(className: string | string[]): this;
    /**
     * Returns the value of the given CSS property of the first node,
     * after applying active stylesheets and resolving any basic computation this value may contain.
     */
    computedCSS(propertyName: string): string;
    /**
     * Returns the value of the given CSS property of the first node, or sets the values of CSS properties for all elements.
     */
    css(propertyName: string): string;
    css(propertyName: KeyValue): this;
    css(propertyName: string, value: string): this;
    /**
     * Returns the width of of the first node.
     */
    width(): number;
    /**
     * Returns the interior width of the first node, which does not include padding.
     */
    innerWidth(): number;
    /**
     * Returns the height of of the first node.
     */
    height(): number;
    /**
     * Displays all nodes.
     */
    show(displayType?: string): this;
    /**
     * Hides all nodes.
     */
    hide(): this;
    /**
     * Returns the HTML string contained within the first node, or sets the HTML string for all elements.
     */
    html(): string;
    html(value: string): this;
    /**
     * Returns the rendered text content of the first node, or sets the rendered text content for all elements.
     */
    text(): string;
    text(value: string): this;
    /**
     * Returns the value of the first node, which must be an input element, or sets the value for all input elements.
     */
    value(): string;
    value(value: string): this;
    /**
     * Returns the HTML string describing the first node including its descendants.
     */
    outerHTML(): string;
    /**
     * Removes all child nodes for each element.
     */
    empty(): this;
    /**
     * Inserts the specified content just inside the first node, before its first child.
     */
    prepend(content: string | Node | DocumentFragment | Nodes): this;
    /**
     * Inserts the specified content just inside the first node, after its last child.
     */
    append(content: string | Node | DocumentFragment | Nodes): this;
    /**
     * Inserts the specified content before the first node.
     */
    before(content: string | Node | DocumentFragment | Nodes): this;
    /**
     * Inserts the specified content after the first node.
     */
    after(content: string | Node | DocumentFragment | Nodes): this;
    /**
     * Replaces the first node with the given new content.
     */
    replaceWith(newContent: string | Node | Nodes): this;
    /**
     * Removes all nodes from the DOM.
     */
    remove(keepChildren?: boolean): this;
    /**
     * Splits the first node, which must be a text node, into two nodes at the specified offset, keeping both as siblings in the tree.
     */
    splitText(offset: number): Nodes;
    /**
     * Returns information about the first node, which is used for debugging.
     */
    toString(): string;
    /**
     * Prints information about the first node, which is used for debugging.
     */
    info(): void;
}

interface TwoParts {
    start: Nodes | null;
    end: Nodes | null;
}
interface ThreeParts extends TwoParts {
    center: Nodes | null;
}
type NodePath = number[];

interface ActiveItem {
    node: Nodes;
    name: string;
    attributes: KeyValue;
    styles: KeyValue;
}
interface SelectionState {
    activeItems: ActiveItem[];
    disabledNameMap?: Map<string, boolean>;
    selectedNameMap?: Map<string, boolean>;
    selectedValuesMap?: Map<string, string[]>;
}

type Translation = RootTranslation;
type RootTranslation = {
    toolbar: {
        /**
         * U​n​d​o​ ​(​m​o​d​+​Z​)
         */
        undo: string;
        /**
         * R​e​d​o​ ​(​m​o​d​+​Y​)
         */
        redo: string;
        /**
         * S​e​l​e​c​t​ ​a​l​l​ ​(​m​o​d​+​A​)
         */
        selectAll: string;
        /**
         * P​a​r​a​g​r​a​p​h
         */
        paragraph: string;
        /**
         * B​l​o​c​k​ ​q​u​o​t​a​t​i​o​n
         */
        blockQuote: string;
        /**
         * N​u​m​b​e​r​e​d​ ​l​i​s​t
         */
        numberedList: string;
        /**
         * B​u​l​l​e​t​e​d​ ​l​i​s​t
         */
        bulletedList: string;
        /**
         * C​h​e​c​k​l​i​s​t
         */
        checklist: string;
        /**
         * A​l​i​g​n​ ​l​e​f​t
         */
        alignLeft: string;
        /**
         * A​l​i​g​n​ ​c​e​n​t​e​r
         */
        alignCenter: string;
        /**
         * A​l​i​g​n​ ​r​i​g​h​t
         */
        alignRight: string;
        /**
         * J​u​s​t​i​f​y
         */
        alignJustify: string;
        /**
         * I​n​c​r​e​a​s​e​ ​i​n​d​e​n​t
         */
        increaseIndent: string;
        /**
         * D​e​c​r​e​a​s​e​ ​i​n​d​e​n​t
         */
        decreaseIndent: string;
        /**
         * B​o​l​d​ ​(​m​o​d​+​B​)
         */
        bold: string;
        /**
         * I​t​a​l​i​c​ ​(​m​o​d​+​I​)
         */
        italic: string;
        /**
         * U​n​d​e​r​l​i​n​e​ ​(​m​o​d​+​U​)
         */
        underline: string;
        /**
         * S​t​r​i​k​e​t​h​r​o​u​g​h
         */
        strikethrough: string;
        /**
         * S​u​p​e​r​s​c​r​i​p​t
         */
        superscript: string;
        /**
         * S​u​b​s​c​r​i​p​t
         */
        subscript: string;
        /**
         * I​n​l​i​n​e​ ​c​o​d​e
         */
        code: string;
        /**
         * R​e​m​o​v​e​ ​f​o​r​m​a​t
         */
        removeFormat: string;
        /**
         * F​o​r​m​a​t​ ​p​a​i​n​t​e​r
         */
        formatPainter: string;
        /**
         * L​i​n​k
         */
        link: string;
        /**
         * H​o​r​i​z​o​n​t​a​l​ ​l​i​n​e
         */
        hr: string;
        /**
         * Y​o​u​T​u​b​e
         */
        video: string;
        /**
         * C​o​d​e​ ​b​l​o​c​k
         */
        codeBlock: string;
        /**
         * H​e​a​d​i​n​g
         */
        heading: string;
        /**
         * H​e​a​d​i​n​g​ ​1
         */
        heading1: string;
        /**
         * H​e​a​d​i​n​g​ ​2
         */
        heading2: string;
        /**
         * H​e​a​d​i​n​g​ ​3
         */
        heading3: string;
        /**
         * H​e​a​d​i​n​g​ ​4
         */
        heading4: string;
        /**
         * H​e​a​d​i​n​g​ ​5
         */
        heading5: string;
        /**
         * H​e​a​d​i​n​g​ ​6
         */
        heading6: string;
        /**
         * L​i​s​t
         */
        list: string;
        /**
         * T​a​b​l​e
         */
        table: string;
        /**
         * A​l​i​g​n​m​e​n​t
         */
        align: string;
        /**
         * I​n​d​e​n​t
         */
        indent: string;
        /**
         * F​o​n​t​ ​f​a​m​i​l​y
         */
        fontFamily: string;
        /**
         * F​o​n​t​ ​s​i​z​e
         */
        fontSize: string;
        /**
         * M​o​r​e​ ​s​t​y​l​e
         */
        moreStyle: string;
        /**
         * F​o​n​t​ ​c​o​l​o​r
         */
        fontColor: string;
        /**
         * H​i​g​h​l​i​g​h​t
         */
        highlight: string;
        /**
         * I​m​a​g​e
         */
        image: string;
        /**
         * V​i​d​e​o
         */
        media: string;
        /**
         * F​i​l​e
         */
        file: string;
        /**
         * E​m​o​j​i
         */
        emoji: string;
        /**
         * M​a​t​h​e​m​a​t​i​c​a​l​ ​f​o​r​m​u​l​a
         */
        equation: string;
        /**
         * X​ ​(​T​w​e​e​t​)
         */
        twitter: string;
        /**
         * R​e​m​o​v​e​ ​c​o​l​o​r
         */
        removeColor: string;
    };
    slash: {
        /**
         * H​e​a​d​i​n​g​ ​1
         */
        heading1: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​h​e​a​d​i​n​g​ ​l​e​v​e​l​ ​1
         */
        heading1Desc: string;
        /**
         * H​e​a​d​i​n​g​ ​2
         */
        heading2: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​h​e​a​d​i​n​g​ ​l​e​v​e​l​ ​2
         */
        heading2Desc: string;
        /**
         * H​e​a​d​i​n​g​ ​3
         */
        heading3: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​h​e​a​d​i​n​g​ ​l​e​v​e​l​ ​3
         */
        heading3Desc: string;
        /**
         * H​e​a​d​i​n​g​ ​4
         */
        heading4: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​h​e​a​d​i​n​g​ ​l​e​v​e​l​ ​4
         */
        heading4Desc: string;
        /**
         * H​e​a​d​i​n​g​ ​5
         */
        heading5: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​h​e​a​d​i​n​g​ ​l​e​v​e​l​ ​5
         */
        heading5Desc: string;
        /**
         * H​e​a​d​i​n​g​ ​6
         */
        heading6: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​h​e​a​d​i​n​g​ ​l​e​v​e​l​ ​6
         */
        heading6Desc: string;
        /**
         * P​a​r​a​g​r​a​p​h
         */
        paragraph: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​p​a​r​a​g​r​a​p​h
         */
        paragraphDesc: string;
        /**
         * B​l​o​c​k​ ​q​u​o​t​a​t​i​o​n
         */
        blockQuote: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​b​l​o​c​k​ ​q​u​o​t​a​t​i​o​n
         */
        blockQuoteDesc: string;
        /**
         * N​u​m​b​e​r​e​d​ ​l​i​s​t
         */
        numberedList: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​n​u​m​b​e​r​e​d​ ​l​i​s​t
         */
        numberedListDesc: string;
        /**
         * B​u​l​l​e​t​e​d​ ​l​i​s​t
         */
        bulletedList: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​b​u​l​l​e​t​e​d​ ​l​i​s​t
         */
        bulletedListDesc: string;
        /**
         * C​h​e​c​k​l​i​s​t
         */
        checklist: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​c​h​e​c​k​l​i​s​t
         */
        checklistDesc: string;
        /**
         * T​a​b​l​e
         */
        table: string;
        /**
         * I​n​s​e​r​t​ ​a​ ​t​a​b​l​e
         */
        tableDesc: string;
        /**
         * I​n​f​o​ ​a​l​e​r​t
         */
        infoAlert: string;
        /**
         * C​r​e​a​t​e​ ​a​n​ ​i​n​f​o​ ​a​l​e​r​t
         */
        infoAlertDesc: string;
        /**
         * T​i​p​ ​a​l​e​r​t
         */
        tipAlert: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​t​i​p​ ​a​l​e​r​t
         */
        tipAlertDesc: string;
        /**
         * W​a​r​n​i​n​g​ ​a​l​e​r​t
         */
        warningAlert: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​w​a​r​n​i​n​g​ ​a​l​e​r​t
         */
        warningAlertDesc: string;
        /**
         * D​a​n​g​e​r​ ​a​l​e​r​t
         */
        dangerAlert: string;
        /**
         * C​r​e​a​t​e​ ​a​ ​d​a​n​g​e​r​ ​a​l​e​r​t
         */
        dangerAlertDesc: string;
        /**
         * H​o​r​i​z​o​n​t​a​l​ ​l​i​n​e
         */
        hr: string;
        /**
         * I​n​s​e​r​t​ ​a​ ​h​o​r​i​z​o​n​t​a​l​ ​l​i​n​e
         */
        hrDesc: string;
        /**
         * C​o​d​e​ ​b​l​o​c​k
         */
        codeBlock: string;
        /**
         * I​n​s​e​r​t​ ​a​ ​c​o​d​e​ ​b​l​o​c​k
         */
        codeBlockDesc: string;
        /**
         * Y​o​u​T​u​b​e
         */
        video: string;
        /**
         * I​n​s​e​r​t​ ​a​ ​Y​o​u​T​u​b​e​ ​v​i​d​e​o
         */
        videoDesc: string;
        /**
         * M​a​t​h​e​m​a​t​i​c​a​l​ ​f​o​r​m​u​l​a
         */
        equation: string;
        /**
         * I​n​s​e​r​t​ ​a​ ​T​e​X​ ​e​x​p​r​e​s​s​i​o​n
         */
        equationDesc: string;
        /**
         * T​w​e​e​t
         */
        twitter: string;
        /**
         * I​n​s​e​r​t​ ​a​n​ ​X​ ​(​T​w​e​e​t​)
         */
        twitterDesc: string;
        /**
         * I​m​a​g​e
         */
        image: string;
        /**
         * U​p​l​o​a​d​ ​a​n​ ​i​m​a​g​e
         */
        imageDesc: string;
        /**
         * V​i​d​e​o
         */
        media: string;
        /**
         * U​p​l​o​a​d​ ​a​ ​v​i​d​e​o
         */
        mediaDesc: string;
        /**
         * F​i​l​e
         */
        file: string;
        /**
         * U​p​l​o​a​d​ ​a​ ​f​i​l​e
         */
        fileDesc: string;
    };
    link: {
        /**
         * N​e​w​ ​l​i​n​k
         */
        newLink: string;
        /**
         * L​i​n​k​ ​U​R​L
         */
        url: string;
        /**
         * T​e​x​t​ ​t​o​ ​d​i​s​p​l​a​y
         */
        title: string;
        /**
         * C​o​p​y​ ​l​i​n​k​ ​t​o​ ​c​l​i​p​b​o​a​r​d
         */
        copy: string;
        /**
         * O​p​e​n​ ​l​i​n​k​ ​i​n​ ​n​e​w​ ​t​a​b
         */
        open: string;
        /**
         * S​a​v​e
         */
        save: string;
        /**
         * R​e​m​o​v​e​ ​l​i​n​k
         */
        unlink: string;
    };
    table: {
        /**
         * F​i​t​ ​t​a​b​l​e​ ​t​o​ ​p​a​g​e​ ​w​i​d​t​h
         */
        fitTable: string;
        /**
         * C​e​l​l​ ​b​a​c​k​g​r​o​u​n​d​ ​c​o​l​o​r
         */
        cellBackground: string;
        /**
         * C​o​l​u​m​n
         */
        column: string;
        /**
         * I​n​s​e​r​t​ ​c​o​l​u​m​n​ ​l​e​f​t
         */
        insertColumnLeft: string;
        /**
         * I​n​s​e​r​t​ ​c​o​l​u​m​n​ ​r​i​g​h​t
         */
        insertColumnRight: string;
        /**
         * D​e​l​e​t​e​ ​c​o​l​u​m​n
         */
        deleteColumn: string;
        /**
         * R​o​w
         */
        row: string;
        /**
         * I​n​s​e​r​t​ ​r​o​w​ ​a​b​o​v​e
         */
        insertRowAbove: string;
        /**
         * I​n​s​e​r​t​ ​r​o​w​ ​b​e​l​o​w
         */
        insertRowBelow: string;
        /**
         * D​e​l​e​t​e​ ​r​o​w
         */
        deleteRow: string;
        /**
         * M​e​r​g​e​ ​c​e​l​l​s
         */
        merge: string;
        /**
         * M​e​r​g​e​ ​c​e​l​l​ ​u​p
         */
        mergeUp: string;
        /**
         * M​e​r​g​e​ ​c​e​l​l​ ​r​i​g​h​t
         */
        mergeRight: string;
        /**
         * M​e​r​g​e​ ​c​e​l​l​ ​d​o​w​n
         */
        mergeDown: string;
        /**
         * M​e​r​g​e​ ​c​e​l​l​ ​l​e​f​t
         */
        mergeLeft: string;
        /**
         * S​p​l​i​t​ ​c​e​l​l
         */
        split: string;
        /**
         * S​p​l​i​t​ ​c​e​l​l​ ​l​e​f​t​ ​a​n​d​ ​r​i​g​h​t
         */
        splitLeftRight: string;
        /**
         * S​p​l​i​t​ ​c​e​l​l​ ​t​o​p​ ​a​n​d​ ​b​o​t​t​o​m
         */
        splitTopBottom: string;
        /**
         * R​e​m​o​v​e​ ​t​a​b​l​e
         */
        remove: string;
    };
    image: {
        /**
         * F​u​l​l​ ​s​c​r​e​e​n
         */
        view: string;
        /**
         * D​e​l​e​t​e
         */
        remove: string;
        /**
         * P​r​e​v​i​o​u​s
         */
        previous: string;
        /**
         * N​e​x​t
         */
        next: string;
        /**
         * C​l​o​s​e​ ​(​E​s​c​)
         */
        close: string;
        /**
         * U​n​a​b​l​e​ ​t​o​ ​l​o​a​d​ ​i​m​a​g​e​.
         */
        loadingError: string;
        /**
         * Z​o​o​m​ ​o​u​t
         */
        zoomOut: string;
        /**
         * Z​o​o​m​ ​i​n
         */
        zoomIn: string;
        /**
         * A​l​i​g​n​m​e​n​t
         */
        align: string;
        /**
         * A​l​i​g​n​ ​l​e​f​t
         */
        alignLeft: string;
        /**
         * A​l​i​g​n​ ​c​e​n​t​e​r
         */
        alignCenter: string;
        /**
         * A​l​i​g​n​ ​r​i​g​h​t
         */
        alignRight: string;
        /**
         * R​e​s​i​z​e​ ​i​m​a​g​e
         */
        resize: string;
        /**
         * P​a​g​e​ ​w​i​d​t​h
         */
        pageWidth: string;
        /**
         * O​r​i​g​i​n​a​l​ ​i​m​a​g​e​ ​w​i​d​t​h
         */
        originalWidth: string;
        /**
         * {​0​}​ ​i​m​a​g​e​ ​w​i​d​t​h
         * @param {unknown} 0
         */
        imageWidth: RequiredParams<'0'>;
        /**
         * O​p​e​n​ ​i​m​a​g​e​ ​i​n​ ​n​e​w​ ​t​a​b
         */
        open: string;
        /**
         * C​a​p​t​i​o​n
         */
        caption: string;
        /**
         * W​r​i​t​e​ ​a​ ​c​a​p​t​i​o​n​.​.​.
         */
        captionPlaceholder: string;
    };
    media: {
        /**
         * D​o​w​n​l​o​a​d
         */
        download: string;
        /**
         * D​e​l​e​t​e
         */
        remove: string;
    };
    file: {
        /**
         * D​o​w​n​l​o​a​d
         */
        download: string;
        /**
         * D​e​l​e​t​e
         */
        remove: string;
    };
    video: {
        /**
         * E​m​b​e​d​ ​v​i​d​e​o
         */
        embed: string;
        /**
         * D​e​l​e​t​e
         */
        remove: string;
        /**
         * P​a​s​t​e​ ​y​o​u​r​ ​Y​o​u​T​u​b​e​ ​l​i​n​k​ ​b​e​l​o​w​.
         */
        description: string;
        /**
         * L​i​n​k
         */
        url: string;
        /**
         * P​l​e​a​s​e​ ​e​n​t​e​r​ ​a​ ​v​a​l​i​d​ ​l​i​n​k​.
         */
        urlError: string;
    };
    codeBlock: {
        /**
         * S​e​l​e​c​t​ ​l​a​n​g​u​a​g​e
         */
        langType: string;
    };
    equation: {
        /**
         * D​o​n​e
         */
        save: string;
        /**
         * S​u​p​p​o​r​t​e​d​ ​f​u​n​c​t​i​o​n​s
         */
        help: string;
        /**
         * T​y​p​e​ ​a​ ​T​e​X​ ​e​x​p​r​e​s​s​i​o​n​.​.​.
         */
        placeholder: string;
    };
    twitter: {
        /**
         * E​m​b​e​d​ ​T​w​e​e​t
         */
        embed: string;
        /**
         * D​e​l​e​t​e
         */
        remove: string;
        /**
         * P​a​s​t​e​ ​y​o​u​r​ ​X​ ​(​T​w​i​t​t​e​r​)​ ​l​i​n​k​ ​b​e​l​o​w​.
         */
        description: string;
        /**
         * L​i​n​k
         */
        url: string;
        /**
         * P​l​e​a​s​e​ ​e​n​t​e​r​ ​a​ ​v​a​l​i​d​ ​l​i​n​k​.
         */
        urlError: string;
    };
};
type TranslationFunctions = {
    toolbar: {
        /**
         * Undo (mod+Z)
         */
        undo: () => LocalizedString;
        /**
         * Redo (mod+Y)
         */
        redo: () => LocalizedString;
        /**
         * Select all (mod+A)
         */
        selectAll: () => LocalizedString;
        /**
         * Paragraph
         */
        paragraph: () => LocalizedString;
        /**
         * Block quotation
         */
        blockQuote: () => LocalizedString;
        /**
         * Numbered list
         */
        numberedList: () => LocalizedString;
        /**
         * Bulleted list
         */
        bulletedList: () => LocalizedString;
        /**
         * Checklist
         */
        checklist: () => LocalizedString;
        /**
         * Align left
         */
        alignLeft: () => LocalizedString;
        /**
         * Align center
         */
        alignCenter: () => LocalizedString;
        /**
         * Align right
         */
        alignRight: () => LocalizedString;
        /**
         * Justify
         */
        alignJustify: () => LocalizedString;
        /**
         * Increase indent
         */
        increaseIndent: () => LocalizedString;
        /**
         * Decrease indent
         */
        decreaseIndent: () => LocalizedString;
        /**
         * Bold (mod+B)
         */
        bold: () => LocalizedString;
        /**
         * Italic (mod+I)
         */
        italic: () => LocalizedString;
        /**
         * Underline (mod+U)
         */
        underline: () => LocalizedString;
        /**
         * Strikethrough
         */
        strikethrough: () => LocalizedString;
        /**
         * Superscript
         */
        superscript: () => LocalizedString;
        /**
         * Subscript
         */
        subscript: () => LocalizedString;
        /**
         * Inline code
         */
        code: () => LocalizedString;
        /**
         * Remove format
         */
        removeFormat: () => LocalizedString;
        /**
         * Format painter
         */
        formatPainter: () => LocalizedString;
        /**
         * Link
         */
        link: () => LocalizedString;
        /**
         * Horizontal line
         */
        hr: () => LocalizedString;
        /**
         * YouTube
         */
        video: () => LocalizedString;
        /**
         * Code block
         */
        codeBlock: () => LocalizedString;
        /**
         * Heading
         */
        heading: () => LocalizedString;
        /**
         * Heading 1
         */
        heading1: () => LocalizedString;
        /**
         * Heading 2
         */
        heading2: () => LocalizedString;
        /**
         * Heading 3
         */
        heading3: () => LocalizedString;
        /**
         * Heading 4
         */
        heading4: () => LocalizedString;
        /**
         * Heading 5
         */
        heading5: () => LocalizedString;
        /**
         * Heading 6
         */
        heading6: () => LocalizedString;
        /**
         * List
         */
        list: () => LocalizedString;
        /**
         * Table
         */
        table: () => LocalizedString;
        /**
         * Alignment
         */
        align: () => LocalizedString;
        /**
         * Indent
         */
        indent: () => LocalizedString;
        /**
         * Font family
         */
        fontFamily: () => LocalizedString;
        /**
         * Font size
         */
        fontSize: () => LocalizedString;
        /**
         * More style
         */
        moreStyle: () => LocalizedString;
        /**
         * Font color
         */
        fontColor: () => LocalizedString;
        /**
         * Highlight
         */
        highlight: () => LocalizedString;
        /**
         * Image
         */
        image: () => LocalizedString;
        /**
         * Video
         */
        media: () => LocalizedString;
        /**
         * File
         */
        file: () => LocalizedString;
        /**
         * Emoji
         */
        emoji: () => LocalizedString;
        /**
         * Mathematical formula
         */
        equation: () => LocalizedString;
        /**
         * X (Tweet)
         */
        twitter: () => LocalizedString;
        /**
         * Remove color
         */
        removeColor: () => LocalizedString;
    };
    slash: {
        /**
         * Heading 1
         */
        heading1: () => LocalizedString;
        /**
         * Create a heading level 1
         */
        heading1Desc: () => LocalizedString;
        /**
         * Heading 2
         */
        heading2: () => LocalizedString;
        /**
         * Create a heading level 2
         */
        heading2Desc: () => LocalizedString;
        /**
         * Heading 3
         */
        heading3: () => LocalizedString;
        /**
         * Create a heading level 3
         */
        heading3Desc: () => LocalizedString;
        /**
         * Heading 4
         */
        heading4: () => LocalizedString;
        /**
         * Create a heading level 4
         */
        heading4Desc: () => LocalizedString;
        /**
         * Heading 5
         */
        heading5: () => LocalizedString;
        /**
         * Create a heading level 5
         */
        heading5Desc: () => LocalizedString;
        /**
         * Heading 6
         */
        heading6: () => LocalizedString;
        /**
         * Create a heading level 6
         */
        heading6Desc: () => LocalizedString;
        /**
         * Paragraph
         */
        paragraph: () => LocalizedString;
        /**
         * Create a paragraph
         */
        paragraphDesc: () => LocalizedString;
        /**
         * Block quotation
         */
        blockQuote: () => LocalizedString;
        /**
         * Create a block quotation
         */
        blockQuoteDesc: () => LocalizedString;
        /**
         * Numbered list
         */
        numberedList: () => LocalizedString;
        /**
         * Create a numbered list
         */
        numberedListDesc: () => LocalizedString;
        /**
         * Bulleted list
         */
        bulletedList: () => LocalizedString;
        /**
         * Create a bulleted list
         */
        bulletedListDesc: () => LocalizedString;
        /**
         * Checklist
         */
        checklist: () => LocalizedString;
        /**
         * Create a checklist
         */
        checklistDesc: () => LocalizedString;
        /**
         * Table
         */
        table: () => LocalizedString;
        /**
         * Insert a table
         */
        tableDesc: () => LocalizedString;
        /**
         * Info alert
         */
        infoAlert: () => LocalizedString;
        /**
         * Create an info alert
         */
        infoAlertDesc: () => LocalizedString;
        /**
         * Tip alert
         */
        tipAlert: () => LocalizedString;
        /**
         * Create a tip alert
         */
        tipAlertDesc: () => LocalizedString;
        /**
         * Warning alert
         */
        warningAlert: () => LocalizedString;
        /**
         * Create a warning alert
         */
        warningAlertDesc: () => LocalizedString;
        /**
         * Danger alert
         */
        dangerAlert: () => LocalizedString;
        /**
         * Create a danger alert
         */
        dangerAlertDesc: () => LocalizedString;
        /**
         * Horizontal line
         */
        hr: () => LocalizedString;
        /**
         * Insert a horizontal line
         */
        hrDesc: () => LocalizedString;
        /**
         * Code block
         */
        codeBlock: () => LocalizedString;
        /**
         * Insert a code block
         */
        codeBlockDesc: () => LocalizedString;
        /**
         * YouTube
         */
        video: () => LocalizedString;
        /**
         * Insert a YouTube video
         */
        videoDesc: () => LocalizedString;
        /**
         * Mathematical formula
         */
        equation: () => LocalizedString;
        /**
         * Insert a TeX expression
         */
        equationDesc: () => LocalizedString;
        /**
         * Tweet
         */
        twitter: () => LocalizedString;
        /**
         * Insert an X (Tweet)
         */
        twitterDesc: () => LocalizedString;
        /**
         * Image
         */
        image: () => LocalizedString;
        /**
         * Upload an image
         */
        imageDesc: () => LocalizedString;
        /**
         * Video
         */
        media: () => LocalizedString;
        /**
         * Upload a video
         */
        mediaDesc: () => LocalizedString;
        /**
         * File
         */
        file: () => LocalizedString;
        /**
         * Upload a file
         */
        fileDesc: () => LocalizedString;
    };
    link: {
        /**
         * New link
         */
        newLink: () => LocalizedString;
        /**
         * Link URL
         */
        url: () => LocalizedString;
        /**
         * Text to display
         */
        title: () => LocalizedString;
        /**
         * Copy link to clipboard
         */
        copy: () => LocalizedString;
        /**
         * Open link in new tab
         */
        open: () => LocalizedString;
        /**
         * Save
         */
        save: () => LocalizedString;
        /**
         * Remove link
         */
        unlink: () => LocalizedString;
    };
    table: {
        /**
         * Fit table to page width
         */
        fitTable: () => LocalizedString;
        /**
         * Cell background color
         */
        cellBackground: () => LocalizedString;
        /**
         * Column
         */
        column: () => LocalizedString;
        /**
         * Insert column left
         */
        insertColumnLeft: () => LocalizedString;
        /**
         * Insert column right
         */
        insertColumnRight: () => LocalizedString;
        /**
         * Delete column
         */
        deleteColumn: () => LocalizedString;
        /**
         * Row
         */
        row: () => LocalizedString;
        /**
         * Insert row above
         */
        insertRowAbove: () => LocalizedString;
        /**
         * Insert row below
         */
        insertRowBelow: () => LocalizedString;
        /**
         * Delete row
         */
        deleteRow: () => LocalizedString;
        /**
         * Merge cells
         */
        merge: () => LocalizedString;
        /**
         * Merge cell up
         */
        mergeUp: () => LocalizedString;
        /**
         * Merge cell right
         */
        mergeRight: () => LocalizedString;
        /**
         * Merge cell down
         */
        mergeDown: () => LocalizedString;
        /**
         * Merge cell left
         */
        mergeLeft: () => LocalizedString;
        /**
         * Split cell
         */
        split: () => LocalizedString;
        /**
         * Split cell left and right
         */
        splitLeftRight: () => LocalizedString;
        /**
         * Split cell top and bottom
         */
        splitTopBottom: () => LocalizedString;
        /**
         * Remove table
         */
        remove: () => LocalizedString;
    };
    image: {
        /**
         * Full screen
         */
        view: () => LocalizedString;
        /**
         * Delete
         */
        remove: () => LocalizedString;
        /**
         * Previous
         */
        previous: () => LocalizedString;
        /**
         * Next
         */
        next: () => LocalizedString;
        /**
         * Close (Esc)
         */
        close: () => LocalizedString;
        /**
         * Unable to load image.
         */
        loadingError: () => LocalizedString;
        /**
         * Zoom out
         */
        zoomOut: () => LocalizedString;
        /**
         * Zoom in
         */
        zoomIn: () => LocalizedString;
        /**
         * Alignment
         */
        align: () => LocalizedString;
        /**
         * Align left
         */
        alignLeft: () => LocalizedString;
        /**
         * Align center
         */
        alignCenter: () => LocalizedString;
        /**
         * Align right
         */
        alignRight: () => LocalizedString;
        /**
         * Resize image
         */
        resize: () => LocalizedString;
        /**
         * Page width
         */
        pageWidth: () => LocalizedString;
        /**
         * Original image width
         */
        originalWidth: () => LocalizedString;
        /**
         * {0} image width
         */
        imageWidth: (arg0: unknown) => LocalizedString;
        /**
         * Open image in new tab
         */
        open: () => LocalizedString;
        /**
         * Caption
         */
        caption: () => LocalizedString;
        /**
         * Write a caption...
         */
        captionPlaceholder: () => LocalizedString;
    };
    media: {
        /**
         * Download
         */
        download: () => LocalizedString;
        /**
         * Delete
         */
        remove: () => LocalizedString;
    };
    file: {
        /**
         * Download
         */
        download: () => LocalizedString;
        /**
         * Delete
         */
        remove: () => LocalizedString;
    };
    video: {
        /**
         * Embed video
         */
        embed: () => LocalizedString;
        /**
         * Delete
         */
        remove: () => LocalizedString;
        /**
         * Paste your YouTube link below.
         */
        description: () => LocalizedString;
        /**
         * Link
         */
        url: () => LocalizedString;
        /**
         * Please enter a valid link.
         */
        urlError: () => LocalizedString;
    };
    codeBlock: {
        /**
         * Select language
         */
        langType: () => LocalizedString;
    };
    equation: {
        /**
         * Done
         */
        save: () => LocalizedString;
        /**
         * Supported functions
         */
        help: () => LocalizedString;
        /**
         * Type a TeX expression...
         */
        placeholder: () => LocalizedString;
    };
    twitter: {
        /**
         * Embed Tweet
         */
        embed: () => LocalizedString;
        /**
         * Delete
         */
        remove: () => LocalizedString;
        /**
         * Paste your X (Twitter) link below.
         */
        description: () => LocalizedString;
        /**
         * Link
         */
        url: () => LocalizedString;
        /**
         * Please enter a valid link.
         */
        urlError: () => LocalizedString;
    };
};

/**
 * The LocaleManager interface manages a collection of Translation objects.
 * It allows you to add and retrieve the names of locales.
 */
declare class LocaleManager {
    /**
     * Adds a Translation to the collection.
     */
    add(locale: string, translation: Translation): void;
    /**
     * Returns a list of all locale names.
     */
    getNames(): string[];
}

type NativeRange = Range;

/**
 * The Range interface represents a fragment of a document that can contain nodes and parts of text nodes.
 * Its interface is similar to the native Range, with some additional properties and methods specifically designed for more efficient manipulation.
 */
declare class Range$1 {
    /**
     * A native Range object.
     */
    private readonly range;
    constructor(range?: NativeRange);
    /**
     * A node within which the range starts.
     */
    get startNode(): Nodes;
    /**
     * A number representing where in the startNode the range starts.
     */
    get startOffset(): number;
    /**
     * A node within which the range ends.
     */
    get endNode(): Nodes;
    /**
     * A number representing where in the endNode the range ends.
     */
    get endOffset(): number;
    /**
     * The deepest node, or the lowest point in the document tree, that contains both boundary points of the range.
     */
    get commonAncestor(): Nodes;
    /**
     * A boolean value indicating whether the range's start and end points are at the same position.
     */
    get isCollapsed(): boolean;
    /**
     * A boolean value indicating whether the range's start point is in a box.
     */
    get isBox(): boolean;
    /**
     * A boolean value indicating whether the commonAncestor is in the start position of a box.
     */
    get isBoxStart(): boolean;
    /**
     * A boolean value indicating whether the commonAncestor is in the center position of a box.
     */
    get isBoxCenter(): boolean;
    /**
     * A boolean value indicating whether commonAncestor is in the end position of a box.
     */
    get isBoxEnd(): boolean;
    /**
     * A boolean value indicating whether commonAncestor is inside the container of a box.
     */
    get isInsideBox(): boolean;
    /**
     * A boolean value indicating whether the range is inoperative.
     */
    get isInoperative(): boolean;
    /**
     * Returns a native Range object from the range.
     */
    get(): NativeRange;
    /**
     * Returns the size and position of the range.
     */
    getRect(): DOMRect;
    /**
     * Returns -1, 0, or 1 depending on whether the specified node is before, the same as, or after the range.
     * −1 if the point is before the range. 0 if the point is in the range. 1 if the point is after the range.
     */
    comparePoint(node: Nodes, offset: number): number;
    /**
     * Returns -1, 0, or 1 depending on whether the beginning of the specified node is before, the same as, or after the range.
     * −1 if the beginning of the node is before the range. 0 if the beginning of the node is in the range. 1 if the beginning of the node is after the range.
     */
    compareBeforeNode(node: Nodes): number;
    /**
     * Returns -1, 0, or 1 depending on whether the end of the specified node is before, the same as, or after the range.
     * −1 if the end of the node is before the range. 0 if the end of the node is in the range. 1 if the end of the node is after the range.
     */
    compareAfterNode(node: Nodes): number;
    /**
     * Returns a boolean value indicating whether the specified node is part of the range or intersects the range.
     */
    intersectsNode(node: Nodes): boolean;
    /**
     * Sets the start position of the range.
     */
    setStart(node: Nodes, offset: number): void;
    /**
     * Sets the start position of the range to the beginning of the specified node.
     */
    setStartBefore(node: Nodes): void;
    /**
     * Sets the start position of the range to the end of the specified node.
     */
    setStartAfter(node: Nodes): void;
    /**
     * Sets the end position of the range.
     */
    setEnd(node: Nodes, offset: number): void;
    /**
     * Sets the end position of the range to the beginning of the specified node.
     */
    setEndBefore(node: Nodes): void;
    /**
     * Sets the end position of the range to the end of the specified node.
     */
    setEndAfter(node: Nodes): void;
    /**
     * Collapses the range to its start.
     */
    collapseToStart(): void;
    /**
     * Collapses the range to its end.
     */
    collapseToEnd(): void;
    /**
     * Sets the range to contain the specified node and its contents.
     */
    selectNode(node: Nodes): void;
    /**
     * Sets the range to contain the contents of the specified node.
     */
    selectNodeContents(node: Nodes): void;
    /**
     * Collapses the range to the center position of the specified box.
     */
    selectBox(boxNode: Nodes): void;
    /**
     * Collapses the range to the start position of the specified box.
     */
    selectBoxStart(boxNode: Nodes): void;
    /**
     * Collapses the range to the end position of the specified box.
     */
    selectBoxEnd(boxNode: Nodes): void;
    /**
     * Collapses the range to the deepest point at the beginning of the contents of the specified node.
     */
    shrinkBefore(node: Nodes): void;
    /**
     * Collapses the range to the deepest point at the end of the contents of the specified node.
     */
    shrinkAfter(node: Nodes): void;
    /**
     * Sets the start and end positions of the range to the deepest start position and end position of the contents of the specified node.
     */
    shrink(): void;
    /**
     * Relocates the start and end positions of the range for boxes.
     */
    adjustBox(): void;
    /**
     * Relocates the start and end positions of the range for tables.
     */
    adjustTable(): void;
    /**
     * Relocates the start and end positions of the range for blocks.
     */
    adjustBlock(): void;
    /**
     * Relocates the start and end positions of the range for boxes, tables, and blocks.
     */
    adjust(): void;
    /**
     * Relocates the start and end positions of the range for <br /> elements.
     */
    adjustBr(): void;
    /**
     * Returns the node immediately preceding the start position of the range.
     */
    getPrevNode(): Nodes;
    /**
     * Returns the node immediately following the end position of the range.
     */
    getNextNode(): Nodes;
    /**
     * Returns the boxes contained within or intersected by the range.
     */
    getBoxes(): Nodes[];
    /**
     * Returns the blocks contained within or intersected by the range.
     */
    getBlocks(): Nodes[];
    /**
     * Returns the marks and text nodes contained within or intersected by the range.
     */
    getMarks(hasText?: boolean): Nodes[];
    /**
     * Returns the text from the start position of the closest block to the start position of the range.
     */
    getStartText(): string;
    /**
     * Returns the text from the end position of the range to the end position of the closest block.
     */
    getEndText(): string;
    /**
     * Returns a new range from the specified character to the start position of the range.
     * The specified character must be preceded by a whitespace or be at the beginning of a paragraph,
     * without being adjacent to other characters. It will return null if not.
     */
    getCharacterRange(character: string): Range$1 | null;
    /**
     * Returns a copy of the range.
     */
    clone(): Range$1;
    /**
     * Returns a DocumentFragment object copying the nodes included in the range.
     */
    cloneContents(): DocumentFragment;
    /**
     * Prints information about the range, which is used for debugging.
     */
    info(): void;
}

/**
 * Inserts a bookmark at the cursor position or a pair of bookmarks at the beginning and end of the range.
 */
declare function insertBookmark(range: Range$1): {
    anchor: Nodes;
    focus: Nodes;
};

/**
 * Changes the specified range to a range represented by the provided bookmark.
 */
declare function toBookmark(range: Range$1, bookmark: {
    anchor: Nodes;
    focus: Nodes;
}): void;

/**
 * The Fragment interface represents a lightweight document object that has no parent.
 * It is designed for efficient manipulation of document structures without affecting the main DOM.
 */
declare class Fragment {
    /**
     * A native DocumentFragment object.
     */
    private readonly fragment;
    constructor(fragment?: DocumentFragment);
    /**
     * Returns a native DocumentFragment object from the fragment.
     */
    get(): DocumentFragment;
    /**
     * Finds and returns descendants of the fragment that match the specified CSS selector.
     */
    find(selector: string): Nodes;
    /**
     * Inserts the specified content just inside the fragment, after its last child.
     */
    append(content: string | Node | Nodes): void;
}

/**
 * Inserts the specified contents into the range.
 */
declare function insertContents(range: Range$1, contents: string | Node | DocumentFragment | Nodes | Fragment): void;

/**
 * Removes the contents of the specified range.
 */
declare function deleteContents(range: Range$1): void;

/**
 * Adds new blocks or changes the target blocks in the specified range.
 */
declare function setBlocks(range: Range$1, value: string | KeyValue): void;

/**
 * Removes the contents of the specified range and then splits the block node at the point of the collapsed range.
 */
declare function splitBlock(range: Range$1): TwoParts;

/**
 * Inserts a block into the specified range.
 */
declare function insertBlock(range: Range$1, value: string | Nodes): Nodes | null;

declare function splitMarks(range: Range$1, removeEmptyMark?: boolean): ThreeParts;

/**
 * Adds the specified mark to the texts of the range.
 */
declare function addMark(range: Range$1, value: string | Nodes): void;

/**
 * Removes the specified marks in the range.
 */
declare function removeMark(range: Range$1, value?: string): void;

/**
 * Inserts a box into the specified range.
 */
declare function insertBox(range: Range$1, boxName: string, boxValue?: BoxValue): Box | null;

/**
 * Removes a box that contains the specified range.
 */
declare function removeBox(range: Range$1): Box | null;

/**
 * The Selection interface represents the range of content selected by the user or the current cursor position.
 */
declare class Selection {
    /**
     * A native Selection object.
     */
    private readonly selection;
    /**
     * A contenteditable element where users can edit the content.
     */
    readonly container: Nodes;
    /**
     * A Range object that can be added to the native selection later.
     */
    range: Range$1;
    constructor(container: Nodes);
    /**
     * Returns a Range object that is currently selected.
     */
    getCurrentRange(): Range$1;
    /**
     * Adds the selection.range to the native selection.
     */
    sync(): void;
    /**
     * Replaces the selection.range with the range of the native selection.
     */
    updateByRange(): void;
    /**
     * Replaces the selection.range with the range represented by the bookmark.
     */
    updateByBookmark(): void;
    /**
     * Returns a list of items related to the current selection.
     */
    getActiveItems(): ActiveItem[];
    /**
     * Creates a deep clone of the current container with its content.
     * If there is a selection within the container, it ensures the selection is also preserved in the cloned container.
     */
    cloneContainer(): Nodes;
    /**
     * Inserts a bookmark at the cursor position or a pair of bookmarks at the selection boundaries.
     */
    insertBookmark(): ReturnType<typeof insertBookmark>;
    /**
     * Changes selection.range to the range represented by the provided bookmark.
     */
    toBookmark(bookmark: Parameters<typeof toBookmark>[1]): ReturnType<typeof toBookmark>;
    /**
     * Inserts the specified content into the selection.
     */
    insertContents(contents: Parameters<typeof insertContents>[1]): ReturnType<typeof insertContents>;
    /**
     * Removes the contents of the selection.
     */
    deleteContents(): ReturnType<typeof deleteContents>;
    /**
     * Adds new blocks or changes the target blocks in the selection.
     */
    setBlocks(value: Parameters<typeof setBlocks>[1]): ReturnType<typeof setBlocks>;
    /**
     * Removes the contents of the selection and splits the block node at the cursor position.
     */
    splitBlock(): ReturnType<typeof splitBlock>;
    /**
     * Inserts a block into the selection.
     */
    insertBlock(value: Parameters<typeof insertBlock>[1]): ReturnType<typeof insertBlock>;
    /**
     * Splits text nodes or mark nodes.
     */
    splitMarks(removeEmptyMark?: Parameters<typeof splitMarks>[1]): ReturnType<typeof splitMarks>;
    /**
     * Adds the specified mark to the selected text.
     */
    addMark(value: Parameters<typeof addMark>[1]): ReturnType<typeof addMark>;
    /**
     * Removes specified marks from the selection.
     */
    removeMark(value?: Parameters<typeof removeMark>[1]): ReturnType<typeof removeMark>;
    /**
     * Collapses the selection to the center position of the specified box.
     */
    selectBox(box: Box | Nodes): void;
    /**
     * Inserts a box into the selection.
     */
    insertBox(boxName: Parameters<typeof insertBox>[1], boxValue?: Parameters<typeof insertBox>[2]): Box;
    /**
     * Removes the specified box. If no parameter is given, the selected box is removed.
     */
    removeBox(box?: Box | Nodes | null): ReturnType<typeof removeBox>;
}

interface CommandItem {
    isDisabled?: (activeItems: ActiveItem[]) => boolean;
    isSelected?: (activeItems: ActiveItem[]) => boolean;
    selectedValues?: (activeItems: ActiveItem[]) => string[];
    execute: (...data: any[]) => void;
}

/**
 * The Command interface manages a collection of commands.
 */
declare class Command {
    private readonly selection;
    private readonly commandMap;
    constructor(selection: Selection);
    /**
     * Adds a new command to the collection.
     */
    add(name: string, commandItem: CommandItem): void;
    /**
     * Removes a command from the collection by its name.
     */
    delete(name: string): void;
    /**
     * Returns the names of all registered commands.
     */
    getNames(): string[];
    /**
     * Checks whether the specified command exists.
     */
    has(name: string): boolean;
    /**
     * Returns a command item by its name.
     */
    getItem(name: string): CommandItem;
    /**
     * Checks if the specified command is disabled.
     */
    isDisabled(name: string): boolean;
    /**
     * Checks if the specified command is selected.
     */
    isSelected(name: string): boolean;
    /**
     * Returns the selected values for the specified command.
     */
    selectedValues(name: string): string[];
    /**
     * Executes the specified command.
     */
    execute(name: string, ...data: any[]): void;
}

interface SaveOptions {
    inputType?: string;
    update?: boolean;
    emitEvent?: boolean;
}
/**
 * The History interface manages undo and redo functionality for a container that holds some editable content.
 * It emits events when actions like save, undo, or redo are performed.
 */
declare class History {
    private readonly selection;
    private readonly container;
    private canSave;
    /**
     * A list in which the current and previous contents are stored.
     */
    readonly list: Nodes[];
    /**
     * A number that always indicates the position at which new content is stored.
     */
    index: number;
    /**
     * The maximum length of the history. Once this limit is reached, the earliest item in the list will be removed.
     */
    limit: number;
    /**
     * A ContentRules object defining the HTML parsing rules used by HTMLParser.
     */
    contentRules: ContentRules;
    /**
     * An EventEmitter object used to set up events.
     */
    readonly event: EventEmitter;
    constructor(selection: Selection);
    private removeBookmark;
    private getValue;
    private addIdToBoxes;
    private removeIdfromBoxes;
    private morphContainer;
    /**
     * A boolean value indicating whether the history can be undone.
     */
    get canUndo(): boolean;
    /**
     * A boolean value indicating whether the history can be redone.
     */
    get canRedo(): boolean;
    /**
     * Undoes to the previous saved content.
     */
    undo(): void;
    /**
     * Redoes to the next saved content.
     */
    redo(): void;
    /**
     * Resumes the ability to save history.
     * This method re-enables saving after the pause method has been called.
     */
    continue(): void;
    /**
     * Pauses the ability to save history.
     * This method temporarily disables saving history, which can be resumed later by calling the continue method.
     */
    pause(): void;
    /**
     * Saves the current content to the history.
     * The content is saved only if it is different from the previous content.
     */
    save(options?: SaveOptions): void;
}

/**
 * The Keystroke interface provides a way to handle keyboard events and define custom shortcuts for a given container.
 * It allows you to register hotkeys, bind specific actions to them, and handle their execution.
 */
declare class Keystroke {
    private readonly container;
    private readonly keydownEventList;
    private readonly keyupEventList;
    constructor(container: Nodes);
    /**
     * Registers a keydown event listener for the specified key combination.
     * The listener will be triggered when the key combination is pressed.
     */
    setKeydown(type: string, listener: EventListener): void;
    /**
     * Registers a keyup event listener for the specified key combination.
     * The listener will be triggered when the key combination is released.
     */
    setKeyup(type: string, listener: EventListener): void;
    /**
     * Triggers all keydown event listeners associated with the specified key combination.
     */
    keydown(type: string): void;
    /**
     * Triggers all keyup event listeners associated with the specified key combination.
     */
    keyup(type: string): void;
}

/**
 * The BoxManager interface manages a collection of BoxComponent objects.
 * It allows you to add, remove, and retrieve the names of components.
 */
declare class BoxManager {
    /**
     * Adds a BoxComponent to the collection.
     */
    add(component: BoxComponent): void;
    /**
     * Removes a box component from the collection by its name.
     */
    remove(name: string): void;
    /**
     * Returns a list of all box component names in the collection.
     */
    getNames(): string[];
}

type UnmountPlugin = () => void;
type InitializePlugin = (editor: Editor) => UnmountPlugin | void;

/**
 * The Plugin interface manages a collection of plugins.
 * It allows plugins to be added and loaded into an Editor instance, and it handles the initialization and unmounting of those plugins.
 */
declare class Plugin {
    private readonly pluginMap;
    /**
     * Registers a plugin using a name as the key.
     */
    add(name: string, plugin: InitializePlugin): void;
    /**
     * Loads all registered plugins.
     */
    loadAll(editor: Editor): Map<string, UnmountPlugin>;
}

type DropdownLocation = 'local' | 'global';
type DropdownDirection = 'top' | 'bottom' | 'auto';
type DropdownMenuType = 'list' | 'icon' | 'character' | 'color';
interface DropdownMenuItem {
    value: string;
    icon?: string;
    text: string | ((locale: TranslationFunctions) => string);
}
interface DropdownItem {
    name: string;
    icon?: string;
    accentIcon?: string;
    downIcon?: string;
    defaultValue?: string;
    tooltip: string | ((locale: TranslationFunctions) => string);
    width?: string;
    menuType: DropdownMenuType;
    menuItems: DropdownMenuItem[];
    menuWidth?: string;
    menuHeight?: string;
    menuCheck?: boolean;
}

interface ToolbarButtonItem {
    name: string;
    type: 'button';
    icon?: string;
    tooltip: string | ((locale: TranslationFunctions) => string);
    isSelected?: (activeItems: ActiveItem[]) => boolean;
    isDisabled?: (activeItems: ActiveItem[]) => boolean;
    onClick: (editor: Editor, value: string) => void;
}
interface ToolbarDropdownItem extends DropdownItem {
    name: string;
    type: 'dropdown';
    selectedValues?: (activeItems: ActiveItem[]) => string[];
    isDisabled?: (activeItems: ActiveItem[]) => boolean;
    onSelect: (editor: Editor, value: string) => void;
}
interface ToolbarUploadItem {
    name: string;
    type: 'upload';
    icon?: string;
    tooltip: string | ((locale: TranslationFunctions) => string);
    accept?: string;
    multiple?: boolean;
}
type ToolbarItem = ToolbarButtonItem | ToolbarDropdownItem | ToolbarUploadItem;

type ToolbarPlacement = 'top' | 'bottom';
interface ToolbarConfig {
    root?: string | Node | Nodes;
    items?: (string | ToolbarItem)[];
    placement?: ToolbarPlacement;
    fontFamily?: {
        defaultValue: string;
        menuItems: DropdownMenuItem[];
    };
}
/**
 * The Toolbar interface provides properties and methods for rendering and manipulating the toolbar.
 */
declare class Toolbar {
    private items;
    private placement;
    private toolbarItemMap;
    private allMenuMap;
    private buttonItemList;
    private dropdownItemList;
    private dropdownList;
    /**
     * The element to which the toolbar is appended.
     */
    root: Nodes;
    /**
     * The element where toolbar items are appended.
     */
    container: Nodes;
    constructor(config: ToolbarConfig);
    private appendDivision;
    private appendNormalButton;
    private appendDropdown;
    private appendUploadButton;
    /**
     * Updates the state of each toolbar item, such as whether it is selected or disabled.
     */
    updateState(state?: SelectionState): void;
    /**
     * Renders the toolbar for the specified editor.
     */
    render(editor: Editor): void;
    /**
     * Destroys the toolbar instance, removing it from the DOM.
     */
    unmount(): void;
}

type ShowMessage = (type: 'success' | 'error' | 'warning', message: string) => void;
type DownloadFile = (type: 'image' | 'media' | 'file', url: string) => void;
interface Config {
    value: string;
    readonly: boolean;
    spellcheck: boolean;
    tabIndex: number;
    placeholder: string;
    indentWithTab: boolean;
    lang: string;
    contentRules: ContentRules;
    minChangeSize: number;
    historySize: number;
    showMessage: ShowMessage;
    downloadFile: DownloadFile;
    [name: string]: any;
}
interface EditorConfig extends Partial<Config> {
    root: string | Node | Nodes;
    toolbar?: Toolbar;
}
/**
 * The Editor interface provides properties and methods for rendering and manipulating the editor.
 */
declare class Editor {
    /**
     * A string that has not yet been saved to the history.
     */
    private unsavedInputData;
    /**
     * The number of input event calls before saving to the history.
     */
    private unsavedInputCount;
    /**
     * The state of the current selection.
     */
    private state;
    /**
     * The functions for unmounting plugins.
     */
    private unmountPluginMap;
    /**
     * The parent element of the container.
     */
    private readonly containerWrapper;
    /**
     * The current version of Lake.
     */
    static readonly version: string;
    /**
     * A LocaleManager object that manages the locale translations.
     */
    static readonly locale: LocaleManager;
    /**
     * A BoxManager object that manages the box components.
     */
    static readonly box: BoxManager;
    /**
     * A Plugin object that manages a collection of plugins.
     */
    static readonly plugin: Plugin;
    /**
     * An element to which the editor is appended.
     */
    readonly root: Nodes;
    /**
     * The toolbar for the editor.
     */
    readonly toolbar: Toolbar | undefined;
    /**
     * The configuration for the editor.
     */
    readonly config: Config;
    /**
     * A contenteditable element where users can edit the editor's content.
     */
    readonly container: Nodes;
    /**
     * An element to which overlays are appended.
     */
    readonly overlayContainer: Nodes;
    /**
     * An EventEmitter object used to set up events.
     */
    readonly event: EventEmitter;
    /**
     * A Selection object representing the range of content selected by the user or the current position of the cursor.
     */
    readonly selection: Selection;
    /**
     * A Command object managing all registered commands.
     */
    readonly command: Command;
    /**
     * A History object that manages the undo and redo history.
     */
    readonly history: History;
    /**
     * A Keystroke object that manages keyboard shortcuts.
     */
    readonly keystroke: Keystroke;
    /**
     * A boolean value indicating whether the editor is in read-only mode.
     */
    readonly readonly: boolean;
    /**
     * A boolean value indicating whether a user is entering a character using a text composition system such as an Input Method Editor (IME).
     */
    isComposing: boolean;
    /**
     * A pop-up component which is currently displayed, such as LinkPopup, MentionMenu, and SlashMenu.
     */
    popup: any;
    constructor(config: EditorConfig);
    private copyListener;
    private cutListener;
    private pasteListener;
    private selectionchangeListener;
    private clickListener;
    private updateSelectionRange;
    /**
     * Updates the classes of all boxes when the current selection is changed.
     */
    private updateBoxSelectionStyle;
    /**
     * Triggers the statechange event when the current selection is changed.
     */
    private emitStateChangeEvent;
    /**
     * Adds or Removes a placeholder class.
     */
    private togglePlaceholderClass;
    /**
     * Moves the input text from box strip to normal position.
     */
    private moveBoxStripText;
    /**
     * Resets the value of "unsavedInputData" property.
     */
    private resetUnsavedInputData;
    /**
     * Handles input event.
     */
    private handleInputEvent;
    /**
     * Binds events for inputting text.
     */
    private bindInputEvents;
    /**
     * Removes all unused box instances.
     */
    private removeBoxGarbage;
    /**
     * Binds events for history.
     */
    private bindHistoryEvents;
    /**
     * Binds events for pointer.
     */
    private bindPointerEvents;
    /**
     * Returns translation functions for the specified language.
     */
    get locale(): TranslationFunctions;
    /**
     * Sets the default config for the specified plugin.
     */
    setPluginConfig(name: string, config: Record<string, any>): void;
    /**
     * Fixes incorrect content, such as adding paragraphs for void elements or removing empty tags.
     */
    fixContent(): boolean;
    /**
     * Renders all boxes that haven't been rendered yet.
     */
    renderBoxes(): void;
    /**
     * Scrolls to the cursor.
     */
    scrollToCursor(): void;
    /**
     * Checks whether the editor has focus.
     */
    hasFocus(): boolean;
    /**
     * Sets focus on the editor.
     */
    focus(): void;
    /**
     * Removes focus from the editor.
     */
    blur(): void;
    /**
     * Returns the state of the current selection.
     */
    getState(): SelectionState;
    /**
     * Sets the specified content to the editor.
     */
    setValue(value: string): void;
    /**
     * Returns the editor's content.
     */
    getValue(): string;
    /**
     * Returns the editor's content in HTML format.
     */
    getHTML(): string;
    /**
     * Renders an editing area and sets default content to it.
     */
    render(): void;
    /**
     * Destroys the editor.
     */
    unmount(): void;
}

interface FloatingToolbarConfig extends ToolbarConfig {
    target: Nodes | Range$1;
}
declare class FloatingToolbar extends Toolbar {
    private range;
    constructor(config: FloatingToolbarConfig);
    private scrollListener;
    private resizeListener;
    private updatePosition;
    render(): void;
    unmount(): void;
}

/**
 * The Box interface represents an embedded content designed to enhance editing capability.
 */
declare class Box {
    /**
     * The lake-box element to which the contents of the box are appended.
     */
    readonly node: Nodes;
    /**
     * An EventEmitter object for handling events. See the Instance events for details.
     */
    readonly event: EventEmitter;
    /**
     * A floating toolbar attached to the box.
     */
    toolbar: FloatingToolbar | null;
    constructor(node: string | Node | Nodes);
    private initiate;
    /**
     * Indicates the type of the box.
     */
    get type(): BoxType;
    /**
     * Returns the name of the box.
     */
    get name(): string;
    /**
     * Gets or sets the value of the box.
     */
    get value(): BoxValue;
    set value(value: BoxValue);
    /**
     * Updates part of the value of the box.
     */
    updateValue(keyName: string, keyValue: string): void;
    updateValue(keyName: BoxValue): void;
    /**
     * Returns the Editor object that contains the box.
     */
    getEditor(): Editor;
    /**
     * Returns the container element of the box.
     */
    getContainer(): Nodes;
    /**
     * Returns the HTML content inside the box.
     */
    getHTML(): string;
    /**
     * Adds a floating toolbar to the box.
     */
    setToolbar(items: ToolbarItem[]): void;
    /**
     * Renders the content inside the box.
     */
    render(): void;
    /**
     * Destroys the box instance.
     */
    unmount(): void;
}

type BoxType = 'inline' | 'block';
type BoxValue = Record<string, any>;
type RenderBox = (box: Box) => Nodes | string | void;
type RenderBoxHTML = (box: Box) => string;
interface BoxComponent {
    type: BoxType;
    name: string;
    value?: BoxValue;
    render: RenderBox;
    html?: RenderBoxHTML;
}

interface CornerToolbarItem {
    name: string;
    icon?: string;
    tooltip: string | ((locale: TranslationFunctions) => string);
    onClick: (event: Event) => void;
}

interface MentionItem {
    id: string;
    name: string;
    nickname?: string;
    avatar?: string;
}

interface SlashButtonItem {
    name: string;
    type: 'button';
    icon?: string;
    title: string | ((locale: TranslationFunctions) => string);
    description: string | ((locale: TranslationFunctions) => string);
    onClick: (editor: Editor, value: string) => void;
}
interface SlashUploadItem {
    name: string;
    type: 'upload';
    icon?: string;
    title: string | ((locale: TranslationFunctions) => string);
    description: string | ((locale: TranslationFunctions) => string);
    accept?: string;
    multiple?: boolean;
}
type SlashItem = SlashButtonItem | SlashUploadItem;

declare const icons: Map<string, string>;

declare function getContentRules(): ContentRules;

/**
 * Returns a Nodes object representing a collection of the nodes.
 * This function is similar to jQuery, but its implementation is very simple.
 * It is designed for simplifying DOM manipulation.
 */
declare function query(content: string | Node | Nodes): Nodes;

/**
 * A tag function that converts all of the reserved characters in the specified string to HTML entities.
 * It also removes empty spaces at the beginning and end of lines.
 */
declare function template(strings: TemplateStringsArray, ...keys: any[]): string;

/**
 * Converts a color in RGB or RGBA format to hex format.
 */
declare function toHex(value: string): string;

/**
 * Returns an existing Box instance associated with the provided boxNode or generates a new one if none exists.
 * The function handles the creation and storage of Box instances, storing them either in a temporary or
 * permanentmap based on whether the boxNode is contained within a container.
 */
declare function getBox(boxNode: string | Node | Nodes): Box;

/**
 * Configuration object that defines the iframe box behavior and appearance.
 */
interface IframeBoxConfig {
    /**
     * The type of the box.
     */
    type: BoxType;
    /**
     * The name of the iframe box component.
     */
    name: string;
    /**
     * The default width of the iframe.
     */
    width: string;
    /**
     * The default height of the iframe.
     */
    height: string;
    /**
     * Description text for the form, which can be localized.
     */
    formDescription: string | ((locale: TranslationFunctions) => string);
    /**
     * Label for the URL input field, which can be localized.
     */
    urlLabel?: string | ((locale: TranslationFunctions) => string);
    /**
     * Placeholder text for the URL input field.
     */
    urlPlaceholder: string;
    /**
     * Text for the embed button, which can be localized.
     */
    embedButtonText: string | ((locale: TranslationFunctions) => string);
    /**
     * Tooltip text for the delete button, which can be localized.
     */
    deleteButtonText: string | ((locale: TranslationFunctions) => string);
    /**
     * Function to validate the inputted URL.
     */
    validUrl: (url: string) => boolean;
    /**
     * Error message shown if URL validation fails.
     */
    urlError: string | ((locale: TranslationFunctions) => string);
    /**
     * Placeholder text shown while the iframe is loading.
     */
    iframePlaceholder?: string;
    /**
     * Function to generate attributes for the iframe element.
     */
    iframeAttributes: (url: string) => Record<string, string>;
    /**
     * Callback executed before the iframe loads.
     */
    beforeIframeLoad?: (box: Box) => void;
    /**
     * If true, allows resizing of the iframe.
     */
    resize?: boolean;
}
/**
 * Creates an iframe box component with configurable properties.
 * This component supports rendering an iframe with customizable attributes, resizing, and toolbar functionalities.
 */
declare function createIframeBox(config: IframeBoxConfig): BoxComponent;

declare function modifierText(value: string, userAgent?: string): string;

/**
 * The HTMLParser interface provides the ability to parse an HTML string according to specified rules.
 */
declare class HTMLParser {
    private readonly rules;
    private readonly source;
    constructor(content: string | Nodes, rules?: ContentRules);
    /**
     * Parses the given HTML string and returns the body element from the result.
     */
    private parseHTML;
    /**
     * Returns a boolean value indicating whether the given value matches the given rule.
     */
    private static matchRule;
    /**
     * Returns an open tag string from the specified element.
     */
    private static getOpenTagString;
    /**
     * Returns a closed tag string from the specified element.
     */
    private static getClosedTagString;
    /**
     * Returns the value of the text node with trimming invisible whitespace.
     */
    private static getTrimmedText;
    /**
     * Returns the parsed HTML as a string.
     */
    getHTML(): string;
    /**
     * Returns the parsed content as a DocumentFragment object.
     */
    getFragment(): DocumentFragment;
}

/**
 * The TextParser interface enables parsing of text into structured HTML.
 */
declare class TextParser {
    private readonly content;
    constructor(content: string);
    /**
     * Converts the parsed text into an HTML string.
     */
    getHTML(): string;
    /**
     * Generates a DocumentFragment object that represents the parsed text.
     */
    getFragment(): DocumentFragment;
}

interface ButtonConfig {
    root: string | Node | Nodes;
    name: string;
    type?: 'primary' | 'default';
    icon?: string;
    text?: string;
    tooltip?: string;
    tabIndex?: number;
    onClick: () => void;
}
/**
 * The Button interface represents a clickable UI component. When a user clicks the button, a specified action is executed.
 */
declare class Button {
    private readonly config;
    private readonly root;
    /**
     * The button element.
     */
    readonly node: Nodes;
    constructor(config: ButtonConfig);
    /**
     * Renders the button to the DOM.
     */
    render(): void;
}

interface DropdownConfig extends DropdownItem {
    root: string | Node | Nodes;
    locale?: TranslationFunctions;
    tabIndex?: number;
    location?: DropdownLocation;
    direction?: DropdownDirection;
    onSelect: (value: string) => void;
}
/**
 * The Dropdown component provides a user-friendly menu with selectable options. Use it to allow users to pick from a list of predefined choices.
 */
declare class Dropdown {
    private readonly config;
    private readonly root;
    private readonly locale;
    private readonly location;
    private readonly direction;
    private readonly menuNode;
    /**
     * The DOM element that contains the dropdown's contents.
     */
    readonly node: Nodes;
    constructor(config: DropdownConfig);
    /**
     * Returns the value of the node.
     */
    static getValue(node: Nodes): string[];
    /**
     * Updates the value of the node.
     */
    static setValue(node: Nodes, value: string[]): void;
    static getMenuMap(menuItems: DropdownMenuItem[], locale: TranslationFunctions): Map<string, string>;
    private updateColorAccent;
    private apppendMenuItems;
    private clickListener;
    private scrollListener;
    private resizeListener;
    private appendMenu;
    private updatePosition;
    private showMenu;
    private hideMenu;
    /**
     * Renders the dropdown to the DOM.
     */
    render(): void;
    /**
     * Removes the dropdown from the DOM and cleans up resources.
     */
    unmount(): void;
}

interface CornerToolbarConfig {
    locale?: TranslationFunctions;
    root: string | Node | Nodes;
    items: CornerToolbarItem[];
}
declare class CornerToolbar {
    private config;
    private locale;
    private root;
    container: Nodes;
    constructor(config: CornerToolbarConfig);
    private appendButton;
    render(): void;
}

interface ResizerConfig {
    root: string | Node | Nodes;
    target: string | Node | Nodes;
    onResize?: (width: number, height: number) => void;
    onStop: (width: number, height: number) => void;
}
declare class Resizer {
    private config;
    private root;
    private target;
    container: Nodes;
    constructor(config: ResizerConfig);
    private bindEvents;
    render(): void;
}

export { Box, Button, CornerToolbar, Dropdown, Editor, Fragment, HTMLParser, Nodes, Range$1 as Range, Resizer, TextParser, Toolbar, addMark, createIframeBox, deleteContents, getBox, getContentRules, icons, insertBlock, insertBookmark, insertBox, insertContents, modifierText, query, removeBox, removeMark, setBlocks, splitBlock, splitMarks, template, toBookmark, toHex };
export type { ActiveItem, BoxComponent, BoxType, BoxValue, CommandItem, ContentRules, CornerToolbarItem, DropdownItem, DropdownMenuItem, EditorConfig, InitializePlugin, KeyValue, MentionItem, NodePath, SelectionState, SlashButtonItem, SlashItem, SlashUploadItem, ToolbarButtonItem, ToolbarConfig, ToolbarDropdownItem, ToolbarItem, ToolbarUploadItem, Translation, TranslationFunctions, UnmountPlugin };
